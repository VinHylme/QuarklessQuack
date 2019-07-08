using MongoDB.Driver;
using MoreLinq;
using Newtonsoft.Json;
using Quarkless.Worker.Actions;
using Quarkless.Worker.TaskScheduler;
using QuarklessContexts.Models.ServicesModels.DatabaseModels;
using QuarklessLogic.Handlers.ClientProvider;
using QuarklessLogic.Handlers.ReportHandler;
using QuarklessRepositories.Repository.ServicesRepositories;
using QuarklessRepositories.Repository.ServicesRepositories.CaptionsRepository;
using QuarklessRepositories.Repository.ServicesRepositories.CommentsRepository;
using QuarklessRepositories.Repository.ServicesRepositories.HashtagsRepository;
using QuarklessRepositories.Repository.ServicesRepositories.UserBiographyRepository;
using QuarklessRepositories.RepositoryClientManager;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using QuarklessContexts.Extensions;
using QuarklessLogic.Handlers.Util;
using ContentSearcher.SeleniumClient;
using static Quarkless.Worker.Analyser;

namespace Quarkless.Worker
{
	public class ActionContainer
	{
		public bool Retry { get; set; }
		public Action Action { get; set; }
		public ActionType Name { get; set; }
	}
	public class Scheduler
	{
		private readonly IReportHandler _reportHandler;
		private readonly ICommentsRepository _commentsRepository;
		private readonly IHashtagsRepository _hashtagsRepository;
		private readonly ICaptionsRepository _captionsRepository;
		private readonly IUserBiographyRepository _userBiographyRepository;
		private readonly IUtilProviders _utilProviders;
		private readonly ConcurrentQueue<IAPIClientContainer> aPIClients;
		private bool goSleep = false;
		private bool producerRan = false;
		private bool notCurrentlyFlushing = true;
		private readonly List<string> Topics = new List<string>
		{
			"Makeup",
			"vape",
			"art",
			"auto",
			"healthcare",
			"skincare",
			"science",
			"tech",
			"memes",
			"gaming",
			"beauty",
			"comics",
			"dance",
			"style",
			"food",
			"decor",
			"diy",
			"nature",
			"fashion",
			"style",
		};
		List<ActionContainer> Devourers { get; set; } = new List<ActionContainer>();
		Dictionary<ActionType, Action> Producer = new Dictionary<ActionType, Action>();
		private WorkerManagerQueue _workerManagerQueue { get; set; }
		private SeleniumClient seleniumClient;

		public Scheduler(ConcurrentQueue<IAPIClientContainer> aPIClient, IUtilProviders util, IRepositoryContext repositoryContext, 
			IReportHandler reportHandler)
		{
			_reportHandler = reportHandler;
			aPIClients = aPIClient;
			_utilProviders = util;
			_commentsRepository = new CommentsRepository(repositoryContext);
			_captionsRepository = new CaptionsRepository(repositoryContext);
			_hashtagsRepository = new HashtagsRepository(repositoryContext);
			_userBiographyRepository = new UserBiographyRepository(repositoryContext);
			_workerManagerQueue = new WorkerManagerQueue(aPIClient);
			_reportHandler.SetupReportHandler("Scheduler");
			SetWorkers();
		}

		private void SetWorkers()
		{
			//adding by percentage (takes from the total number avaliable) so not 40 workers but 40% of total workers
			_workerManagerQueue.AddWorkers(20, WorkerType.Fetcher);
			_workerManagerQueue.AddWorkers(80, WorkerType.Extracter);
		}
		public void ScheduleProducer(int limit)
		{
			Task.Run(async () =>
			{
				foreach(var topic in Topics)
				{
					var res = await MediaTask(topic,limit);
					if (res)
					{
						continue;
					}
				}
			}).ContinueWith(t=>
			{
				//_workerManagerQueue.RequestWorker(WorkerType.Extracter, WorkerType.Fetcher, Producer.Count-1);
				producerRan = false;
			});
		}
		public void ScheduleDevour(int counter, ActionType type, params IServiceRepository[] serviceRepositories)
		{
			Task.Run(async () =>
			{
				var results = await _workerManagerQueue.Run(counter,type,_reportHandler,serviceRepositories);
				switch (results)
				{
					case JobStatusResponse.Retry:
						Devourers.Where(_=>_.Name == type).SingleOrDefault().Retry = true;
						await Task.Delay(TimeSpan.FromMinutes(1.2)).ContinueWith(a=>Devourers.Where(_=>_.Name == type).SingleOrDefault().Retry = false);
						break;
					case JobStatusResponse.Fail:
						break;
					case JobStatusResponse.Finished:
						_workerManagerQueue.DeleteJobs();
						break;
				}
			}).ContinueWith(ea=>{ });
		}
		private Action CreateFunction(ActionType action, int limit)
		{
			switch (action)
			{
				case ActionType.FetchBiography:
					return ()=> ScheduleDevour(limit,action,_userBiographyRepository);
				case ActionType.FetchCaptions:
					return ()=> ScheduleDevour(limit,action,_captionsRepository,_hashtagsRepository);
				case ActionType.FetchComments:
					return ()=>ScheduleDevour(limit,action,_commentsRepository,_hashtagsRepository);
				case ActionType.FetchMedia:
					return ()=> ScheduleProducer(limit);
				default:
					break;
			}
			return null;
		}
		private void SetTimers(int sleepTime, int restartTime, int flushTime, int totalItemsToFlush)
		{
			System.Timers.Timer timer = new System.Timers.Timer(sleepTime);
			timer.Start();
			timer.Elapsed += (o, s) =>
			{
				goSleep = !goSleep;
				_workerManagerQueue.Restart();
			};

			var timerrestart = new System.Timers.Timer(restartTime);
			timerrestart.Start();
			timerrestart.Elapsed += (o, s) =>
			{
				_workerManagerQueue.Restart();
			};

			var flushTimer = new System.Timers.Timer(flushTime);
			flushTimer.Start();
			flushTimer.Elapsed += (o, s) =>
			{
				if (notCurrentlyFlushing) { 
				Flush(totalItemsToFlush);
				}
			};

		}
		public void Schedule(TimerSettings timerSettings, ExecutionSettings executionSettings, params ActionExecuters[] actionExecuters)
		{

			if (actionExecuters.Count()<=0) throw new Exception("actions should exist");
			foreach(var action in actionExecuters)
			{
				if(action.WorkerType == WorkerType.Fetcher)
				{
					Producer.Add(action.Action,CreateFunction(action.Action,action.Limit));
				}
				else if(action.WorkerType == WorkerType.Extracter)
				{
					Devourers.Add(new ActionContainer { Name = action.Action, Action = CreateFunction(action.Action, action.Limit)});
				}
			}

			bool isWaiting = false;
			SetTimers(
				timerSettings.SleepTime,timerSettings.RestartTime,
				timerSettings.FlushTime,executionSettings.TotalItemsToFlushPerFlush
			);

			int actionDevourPosition = 0;
			int actionProducePosition = 0;
			while (true)
			{
				if (!goSleep) { 
					if (_workerManagerQueue.JobCount > 0) {
						if (actionDevourPosition > Devourers.Count - 1)
							actionDevourPosition = 0;
						else { 
							var function = Devourers.ElementAt(actionDevourPosition++);	
							if(!function.Retry)
								function.Action();
							Thread.Sleep(TimeSpan.FromSeconds(executionSettings.ThreadRefreshFrame));
						}
					}
					else if (_workerManagerQueue.JobCount <= timerSettings.ProducerThreshHoldLimit && !producerRan)
					{
						if(actionProducePosition > Producer.Count-1)
							actionProducePosition = 0;

						producerRan = true;
						Producer.ElementAt(actionProducePosition++).Value();
						//_workerManagerQueue.RequestWorker(WorkerType.Fetcher, WorkerType.Extracter, Producer.Count - 1);

					}

					if (!isWaiting) { 
						isWaiting = true;
						Task.Run(async () => { 
							await Task.Delay(timerSettings.TransferingWorkerCheckTime).ContinueWith(ta =>
							{
								_workerManagerQueue.TransferWorkers();
								isWaiting = false;
							});
						});
					}
				}
			}
		}
		public async Task<bool> MediaTask(string topic, int limit)
		{
			var res = await _workerManagerQueue.RunProducer(WorkerType.Fetcher, ActionType.FetchMedia, _reportHandler, topic, limit);
			if (res != null)
			{
				return true;
			}
			return false;
		}
		private Task Work<TObject>(IEnumerable<TObject> items, string fileNameCSV, string fileNameJsonActualData)
		{
			if (items.Count() <= 0) return null;
			return Task.Run(() =>
			{
				items = HandleMultiTranslate(items,items.Count()/2);

				var dtc = items.Select(o =>
				{
					return new DataHolder()
					{
						Text = o.GetValue("Text").ToString(),
						Topic = o.GetValue("Topic").ToString(),
						Language = o.GetValue("Language")?.ToString()
					};
				}).ToDataTable(
						a => a.Text,
						b => b.Topic,
						c => c.Language);
				dtc.WriteToCsvFile(fileNameCSV);
			}).ContinueWith(a =>
			{
				items.SaveAsJSON(fileNameJsonActualData);
				if (items is IEnumerable<CaptionsModel>)
				{
					Console.WriteLine("Deleting Captions...");
					_captionsRepository.RemoveCaptions(items.Select(_ => _.GetType().GetProperty("_id").GetValue(_).ToString()));
					Console.WriteLine("Captions Deleted");
				}
				else if (items is IEnumerable<CommentsModel>)
				{
					Console.WriteLine("Deleting Comments...");
					_commentsRepository.RemoveComments(items.Select(_ => _.GetType().GetProperty("_id").GetValue(_).ToString()));
					Console.WriteLine("Deleted Comments");
				}
				else if (items is IEnumerable<UserBiographyModel>)
				{
					Console.WriteLine("Deleting User Biographies...");
					_userBiographyRepository.RemoveUserBiographies(items.Select(_ => _.GetType().GetProperty("_id").GetValue(_).ToString()));
					Console.WriteLine("Deleted User Biographies");
				}
			});
		}
		private IEnumerable<TObject> HandleMultiTranslate<TObject>(IEnumerable<TObject> items, int seperateBy = 255)
		{
			try
			{
				int incrementedBy = 0;
				int amountToLoop = items.Count() >= seperateBy
					? items.Where(_ => !string.IsNullOrEmpty(_.GetValue("Text").ToString())).Count() / seperateBy
					: items.Where(_ => !string.IsNullOrEmpty(_.GetValue("Text").ToString())).Count();

				List<Holder<TObject>> workQueue = new List<Holder<TObject>>();

				for (int i = 0; i < amountToLoop; i++)
				{
					workQueue.Add(new Holder<TObject>
					{
						Items = items.TakeBetween(incrementedBy, seperateBy).ToList(),
					});
					incrementedBy += seperateBy;
				}

				Parallel.ForEach(workQueue, fn =>
				{
					Task.Delay(250);
					fn.Langs = _utilProviders.TranslateService.DetectLanguageViaGoogle(texts: fn.Items
						.Select(c => c.GetValue("Text").ToString()).ToArray());
					if (fn.Langs != null)
					{
						for (int x = 0; x < fn.Items.Where(a => (!string.IsNullOrEmpty(a.GetValue("Text").ToString()))).Count(); x++)
						{
							fn.Items.ElementAt(x).GetProp("Language")
							.SetValue(fn.Items.ElementAt(x), fn.Langs.ElementAtOrDefault(x));
						}
					}
					else
					{

					}
					fn.IsDone = true;
				});

				return workQueue.Select(t => t.Items).Squash();
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
				return null;
			}
		}

		/// <summary>
		/// Clear out database so files do not get too massive (save cost)
		/// size -1 = All
		/// </summary>
		/// <param name="size"></param>
		public void Flush(int size = -1) 
		{
			notCurrentlyFlushing = false;
			var t1 = Work(_userBiographyRepository.GetUserBiographies(limit:-1).GetAwaiter().GetResult(),
				@"C:\Users\yousef.alaw\source\repos\QuarklessQuark\Requires\Datas\normalised_data\_bios.csv",
				$@"C:\Users\yousef.alaw\source\repos\QuarklessQuark\Requires\Datas\data_bk\Bios\{DateTime.UtcNow.ToLongDateString()}_{Guid.NewGuid()}_bios_new.json");

			var t2 = Work(_captionsRepository.GetCaptions(limit:-1).GetAwaiter().GetResult(),
				@"C:\Users\yousef.alaw\source\repos\QuarklessQuark\Requires\Datas\normalised_data\_captions.csv",
				$@"C:\Users\yousef.alaw\source\repos\QuarklessQuark\Requires\Datas\data_bk\Captions\{DateTime.UtcNow.ToLongDateString()}_{Guid.NewGuid()}_captions_new.json"
				);

			var t3 = Work(_commentsRepository.GetComments(limit:-1).GetAwaiter().GetResult(), 
				@"C:\Users\yousef.alaw\source\repos\QuarklessQuark\Requires\Datas\normalised_data\_comments.csv",
				$@"C:\Users\yousef.alaw\source\repos\QuarklessQuark\Requires\Datas\data_bk\Comments\{DateTime.UtcNow.ToLongDateString()}_{Guid.NewGuid()}_comments_new.json");

			Task.WaitAll(t1, t2, t3);
			notCurrentlyFlushing = true;
		}
	}

	#region oldCode
	/*
 * 		private void SaveThenRemove<T>(string filePathJson,string filePathCSV, Func<Task<IEnumerable<T>>> objectToFlush)
{
	IEnumerable<T> result = null;
	try { 
		Task.Run(async () =>
		{
			result = await objectToFlush();
		}).ContinueWith(a =>
		{
			if (result != null)
			{
				Console.WriteLine($"Saving file to {filePathJson}");
				result.SaveAsJSON(filePathJson);
				var dtc = result.Select(o =>
				{
					var text = o.GetValue("Text").ToString();
					var topic = o.GetValue("Topic").ToString();
					var lang = o.GetValue("Language")?.ToString();
					if(string.IsNullOrWhiteSpace(topic) || string.IsNullOrWhiteSpace(lang))
					{
						return new DataHolder();
					}
					return new DataHolder()
					{
						Text = text,
						Topic = topic,
						Language = lang
					};
				}).Where(_=>!string.IsNullOrEmpty(_.Text)).ToDataTable(
				a => a.Text,
				b => b.Topic,
				c => c.Language);

				dtc.WriteToCsvFile(filePathCSV);
				if (result is IEnumerable<CaptionsModel>)
				{
					Console.WriteLine("Deleting Captions...");
					_captionsRepository.RemoveCaptions(result.Select(_=>_.GetType().GetProperty("_id").GetValue(_).ToString()));
					Console.WriteLine("Captions Deleted");
				}
				else if(result is IEnumerable<CommentsModel>)
				{
					Console.WriteLine("Deleting Comments...");
					_commentsRepository.RemoveComments(result.Select(_ => _.GetType().GetProperty("_id").GetValue(_).ToString()));
					Console.WriteLine("Deleted Comments");
				}
				else if(result is IEnumerable<UserBiographyModel>)
				{
					Console.WriteLine("Deleting User Biographies...");
					_userBiographyRepository.RemoveUserBiographies(result.Select(_ => _.GetType().GetProperty("_id").GetValue(_).ToString()));
					Console.WriteLine("Deleted User Biographies");
				}
			}
		});
	}
	catch(Exception e)
	{
		Console.WriteLine(e.Message);
	}
}

private async Task<IEnumerable<CaptionsModel>> HandleCaptions(int limit)
{
	var captions = await _captionsRepository.GetCaptions(limit:limit);
	if(captions.Count()<=0) return null;
	List<string> langs = new List<string>();
	const int minumum = 255;
	int incrementBy = 0;

	int amountToLoop = captions.Count()>minumum 
		? captions.Where(_=>!string.IsNullOrEmpty(_.Text)).Count() / minumum
		: captions.Where(_ => !string.IsNullOrEmpty(_.Text)).Count();

	for (int i = 0 ; i < amountToLoop; i++)
	{
		var between = captions.TakeBetween(incrementBy, minumum);
		if (between.Count() > 0) { 
			langs.AddRange(_utilProviders.TranslateService.DetectLanguageViaGoogle(texts:between.Select(c=>c.Text).ToArray()));
			incrementBy += minumum;
			await Task.Delay(TimeSpan.FromSeconds(5));
		}
	}
	for(int x = 0 ; x < captions.Where(a=>!string.IsNullOrEmpty(a.Text)).Count(); x++)
	{
		captions.ToList()[x].Language = langs.ElementAtOrDefault(x);
	}

	return captions;
}
private async Task<IEnumerable<CommentsModel>> HandleComments(int limit)
{
	var comments = await _commentsRepository.GetComments(limit: limit);
	if(comments.Count()<=0) return null;
	List<string> langs = new List<string>();
	const int minumum = 255;
	int incrementBy = 0;

	int amountToLoop = comments.Count() > minumum
		? comments.Where(_ => !string.IsNullOrEmpty(_.Text)).Count() / minumum
		: comments.Where(_ => !string.IsNullOrEmpty(_.Text)).Count();

	for (int i = 0; i < amountToLoop; i++)
	{
		var between = comments.TakeBetween(incrementBy, minumum);
		if (between.Count() > 0)
		{
			langs.AddRange(_utilProviders.TranslateService.DetectLanguageViaGoogle(texts: between.Select(c => c.Text).ToArray()));
			incrementBy += minumum;
			await Task.Delay(TimeSpan.FromSeconds(3));
		}
	}
	for (int x = 0; x < comments.Where(a => !string.IsNullOrEmpty(a.Text)).Count(); x++)
	{
		comments.ToList()[x].Language = langs.ElementAtOrDefault(x);
	}

	return comments;
}
private async Task<IEnumerable<UserBiographyModel>> HandleBiographies(int limit)
{
	var bios = await _userBiographyRepository.GetUserBiographies(limit: limit);
	if(bios.Count()<=0) return null;
	List<string> langs = new List<string>();
	const int minumum = 255;
	int incrementBy = 0;

	int amountToLoop = bios.Count() > minumum
		? bios.Where(_ => !string.IsNullOrEmpty(_.Text)).Count() / minumum
		: bios.Where(_ => !string.IsNullOrEmpty(_.Text)).Count();

	for (int i = 0; i < amountToLoop; i++)
	{
		var between = bios.TakeBetween(incrementBy, minumum);
		if (between.Count() > 0)
		{
			langs.AddRange(_utilProviders.TranslateService.DetectLanguageViaGoogle(texts: between.Select(c => c.Text).ToArray()));
			incrementBy += minumum;
			await Task.Delay(TimeSpan.FromSeconds(3));
		}
	}
	for (int x = 0; x < bios.Where(a => !string.IsNullOrEmpty(a.Text)).Count(); x++)
	{
		bios.ToList()[x].Language = langs.ElementAtOrDefault(x);
	}

	return bios;
}
*/
	#endregion
}
