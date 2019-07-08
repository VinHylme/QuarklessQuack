using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using Quarkless.Common;
using System.Collections.Generic;
using System.Threading.Tasks;
using Quarkless.Worker.WorkerPool;
using Quarkless.Worker.Actions;
using Quarkless.Worker.TaskScheduler;
using System.Collections.Concurrent;
using QuarklessLogic.Handlers.ClientProvider;
using QuarklessLogic.Handlers.Util;
using QuarklessLogic.Handlers.TextGeneration;

namespace Quarkless.Worker
{
	public enum SchedulerPriority
	{
		Highest,
		High,
		Medium,
		Low
	}
	public class ActionExecuters
	{
		public WorkerType WorkerType { get; set; }
		public ActionType Action { get; set; }
		public int Limit { get; set; }
		public SchedulerPriority Priority { get; set; }
	}
	public class TimerSettings
	{
		public int SleepTime { get; set; }
		public int RestartTime { get; set; }
		public int FlushTime { get; set; }
		public int TransferingWorkerCheckTime { get; set; }
		public int ProducerThreshHoldLimit { get; set; }
	}
	public class UserAccount
	{
		public string AccountId { get; set; }
		public ConcurrentQueue<IAPIClientContainer> APIClients;
		public UserAccount()
		{
			APIClients = new ConcurrentQueue<IAPIClientContainer>();
		}
	}
	public class ExecutionSettings
	{
		public double ThreadRefreshFrame { get; set; }
		public int TotalItemsToFlushPerFlush { get; set; }
	}
	public class SchedulerSettings
	{
		public List<UserAccount> Accounts { get; set; }
		public List<ActionExecuters> ActionExecuters { get; set; }
		public TimerSettings TimerSettings { get; set; }
		public ExecutionSettings ExecutionSettings { get;set;}
		public SchedulerSettings()
		{
			ActionExecuters = new List<ActionExecuters>();
		}
	}
	class Program
	{
		//Future implementation idea, instead of having a single sleep time for all worker threads,
		//create seperate ones for each worker (workShift) this way it will be more random and workers will have different states
		//Idle, Working, Sleeping
		//Possibly implement the scheduler priority for each type of action, this way you can give more importance to specific jobs
		static void Main(string[] args)
		{
			bool shouldNormalise = false;

			var settingPath = Path.GetFullPath(Path.Combine(@"..\..\..\..\Quarkless")); 
			IConfiguration configuration = new ConfigurationBuilder().
				SetBasePath(settingPath).AddJsonFile("appsettings.json").Build();

			Accessors accessors = new Accessors(configuration);
			var services = new ServiceCollection();
			var glcred = accessors.GoogleCredentials($@"{settingPath}\translatedetect-9663e6daa381.json");
			services.AddLogics();
			services.AddContexts();
			services.AddHandlers();
			services.AddRepositories(accessors);	
			services.AddSingleton<IWorker,WorkerPool.Worker>();

			Servicor service = new Servicor(services.BuildServiceProvider());
			if (!shouldNormalise) { 
				SchedulerSettings schedulerSettings = new SchedulerSettings
				{
					Accounts = new List<UserAccount> 
					{ 
						new UserAccount
						{
							AccountId = "lemonkaces"
						}
					},
					TimerSettings = new TimerSettings { 
						RestartTime = (int) TimeSpan.FromMinutes(18).TotalMilliseconds,
						SleepTime = (int) TimeSpan.FromHours(2).TotalMilliseconds,
						FlushTime = (int) TimeSpan.FromMinutes(30).TotalMilliseconds,
						ProducerThreshHoldLimit = 0,
						TransferingWorkerCheckTime = (int) TimeSpan.FromSeconds(5).TotalMilliseconds
					},
					ExecutionSettings = new ExecutionSettings { 
						TotalItemsToFlushPerFlush = -1, //all
						ThreadRefreshFrame = 0.55
					},
					ActionExecuters = new List<ActionExecuters>
					{
						new ActionExecuters
						{
							Action = ActionType.FetchComments,
							Limit = 4,
							Priority = SchedulerPriority.High,
							WorkerType = WorkerType.Extracter
						},
						new ActionExecuters
						{
							Action = ActionType.FetchBiography,
							Limit = 4,
							Priority = SchedulerPriority.High,
							WorkerType = WorkerType.Extracter
						},                  
						new ActionExecuters
						{
							Action = ActionType.FetchCaptions,
							Limit = 45,
							Priority = SchedulerPriority.High,
							WorkerType = WorkerType.Extracter
						},
						new ActionExecuters
						{
							Action = ActionType.FetchMedia,
							Limit = 2,
							Priority = SchedulerPriority.Highest,
							WorkerType = WorkerType.Fetcher
						}
					}
				};
				var task = Task.Run(async () =>
				{
					await service.Get<IWorker>().Begin(schedulerSettings);
				});

				Task.WaitAll(task);
			}
			else
			{
				Analyser analyser = new Analyser("../../../Data/data_bk/",service.Get<IUtilProviders>());
				analyser.Start();
			}
		}
	}
	public class Servicor 
	{
		private readonly IServiceProvider _serviceProvider;
		public Servicor(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}
		public TInstance Get<TInstance>() =>_serviceProvider.GetService<TInstance>();
	}
}
