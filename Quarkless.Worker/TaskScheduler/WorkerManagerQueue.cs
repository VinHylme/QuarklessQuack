using InstagramApiSharp.Classes.Models;
using MoreLinq;
using MoreLinq.Extensions;
using Quarkless.Worker.Actions;
using Quarkless.Worker.Models;
using Quarkless.Worker.TaskScheduler;
using QuarklessLogic.Handlers.ClientProvider;
using QuarklessLogic.Handlers.ReportHandler;
using QuarklessLogic.Handlers.Util;
using QuarklessRepositories.Repository.ServicesRepositories;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quarkless.Worker
{
	public enum JobStatusResponse
	{
		Finished,
		Retry,
		Fail,
		NotReady
	}
	public class TransferWorkerRequest
	{
		public WorkerAPI fromWorker { get; set; }
		public WorkerAPI toWorker { get; set; }
	}
	public static class WorkerManagerExtension
	{
		public static ConcurrentQueue<T> ToConcurrentQueue <T>(this IEnumerable<T> @items)
		{
			ConcurrentQueue<T> queue = new ConcurrentQueue<T>();
			foreach(var item in @items) {
				queue.Enqueue(item);
			}
			return queue;
		}

		public static T FindItem<T>(this IEnumerable<T> @items, T search)
		{
			if (@items.Count() <= 0) { return default(T);}
			foreach(var item in @items)
			{
				if(item.Equals(search))
				{
					return item;
				}
			}
			return default(T);
		}

	}
	public class WorkerManagerQueue
	{
		private readonly ConcurrentQueue<IAPIClientContainer> _workerAPI;
		private List<WorkerAPI> Workers { get; set; }
		private List<WorkerAPI> OGWorkers { get; set; }
		private Dictionary<string,WorkerAPI> ActiveJobs { get; set; }
		private List<TransferWorkerRequest> TransferList { get; set; }
		private List<JobItem> JobItems { get;set; } = new List<JobItem>();
		public WorkerManagerQueue(ConcurrentQueue<IAPIClientContainer> workers)
		{
			_workerAPI = workers;
			Workers = new List<WorkerAPI>();
			ActiveJobs = new Dictionary<string, WorkerAPI>();
			OGWorkers = new List<WorkerAPI>();
			TransferList = new List<TransferWorkerRequest>();
		}
		public int TotalWorkersAvaliable
		{
			get
			{
				return Workers.Count;
			}
		}
		public int JobCount
		{
			get
			{
				return JobItems.Count;
			}
		}
		public bool TransferWorkers()
		{
			try {
				if (TransferList.Count < 1) { return false; }
				foreach(var transfer in TransferList) { 
					Workers.Remove(transfer.fromWorker);
					Workers.Add(transfer.toWorker);
				}

				TransferList.Clear();
				return true;
			}
			catch(Exception e)
			{ 
				return false;
			}
		}
		public bool RequestWorker(WorkerType requestFor, WorkerType Takefrom, int numberOfWorkersRequired = 1)
		{
			var reqfromworker = GetAllWorkersByType(Takefrom);
			if(reqfromworker==null && reqfromworker.Count < numberOfWorkersRequired)
			{
				return false;
			}

			var totake = reqfromworker.Where(_=>_._WorkerType == Takefrom).Take(numberOfWorkersRequired).Select((_,i)=> 
			{
				var taken = new WorkerAPI(_._worker, requestFor);
				TransferWorkerRequest transfer = new TransferWorkerRequest
				{
					fromWorker = _,
					toWorker = taken
				};
				return transfer; 
			});
			if (totake == null) { return false;}
			TransferList.AddRange(totake);
			return true;
		}
		public List<WorkerAPI> GetWorkers()
		{
			return Workers;
		}
		public List<WorkerAPI> GetAllWorkersByType(WorkerType workerType)
		{
			return Workers.Where(_=>_._WorkerType == workerType).ToList();
		}
		private WorkerAPI Take(WorkerType workert)
		{
			lock (Workers) {  
				if (Workers.Count > 0)
				{
					var worker = Workers.Where(_=>_._WorkerType == workert).FirstOrDefault();
					if (worker == null) { return null;}
					Workers.Remove(worker);
					return worker;
				}
				return null;
			}
		}
		public WorkerAPI GetWorkerByType(WorkerType workerType)
		{
			WorkerAPI workerAPI = Take(workerType);
			if (workerAPI != null)
			{
				return workerAPI;
			}
			return null;
		}
		public void AddWorker(WorkerAPI worker)
		{
			lock (Workers) { 
				Workers.Add(worker);
			}
		}
		public WorkerAPI GetWorker(int id)
		{
			return Workers.ElementAtOrDefault(id);
		}
		public void AddWorkers(double perctange, WorkerType worker)
		{
			if(perctange < 0) throw new Exception("percetange cannot be 0");
			if(perctange > 100) throw new Exception("percentage cannot be over 100");
			if(_workerAPI.Count < 1) throw new Exception("not enough slaves");
			int numberOfWorkers = ((int)Math.Ceiling(((double)_workerAPI.Count) * (perctange / 100)));
			for( int i = 0 ; i < numberOfWorkers; i++)
			{
				IAPIClientContainer container = null;
				_workerAPI.TryDequeue(out container);

				if (container != null) { 
					Workers.Add(new WorkerAPI(container,worker)); 
					OGWorkers.Add(new WorkerAPI(container,worker));
				}
			}
		}
		public void Restart()
		{
			lock (Workers) { 
				Workers.Clear();
				Workers.AddRange(OGWorkers);
			}
		}
		public async Task<JobStatusResponse> Run(int offset, ActionType type, IReportHandler reportHandler, params IServiceRepository[] serviceRepositories)
		{
			try
			{
				if (JobItems.Count <= 0) return JobStatusResponse.NotReady;
				if (serviceRepositories == null) return JobStatusResponse.NotReady;
				List<JobItem> jobLimit = JobItems.Where(_ => _.AssignedItems.JobStatus == JobStatus.NotStarted && 
				_.AssignedWorker == type).Take(offset).ToList();
				if (jobLimit.Count <= 0) return JobStatusResponse.NotReady;

				var worker_ = GetWorkerByType(WorkerType.Extracter);
				if (worker_ == null) { return JobStatusResponse.NotReady; }

				var date = DateTime.UtcNow;
				Console.WriteLine(worker_._IdIndex+"::" + date);
				ActiveJobs.Add(worker_._IdIndex + "," + date, worker_);

				SetStatus(jobLimit, JobStatus.InProgress);

				var resuts = (bool)await worker_.Execute(
						jobLimit.First().AssignedWorker,
						reportHandler,
						jobLimit.First().AssignedItems.Topic,
						jobLimit.First().AssignedItems.Limit,
						jobLimit.Select(_ => _.AssignedItems.Item).ToList(),
						serviceRepositories);

				lock (Workers) { 
					AddWorker(worker_);
					ActiveJobs.Remove(worker_._IdIndex + "," + date);
				}
				if (resuts)
				{
					SetStatus(jobLimit, JobStatus.Finished);
					return JobStatusResponse.Finished;
				}
				else
				{
					SetStatus(jobLimit, JobStatus.NotStarted);
					return JobStatusResponse.Retry;
				}
			}
			catch(Exception ee)
			{
				Console.WriteLine(ee.Message);
				return JobStatusResponse.Fail;
			}
			finally
			{
				if (ActiveJobs.Count > 0)
				{
					ActiveJobs.Select(_ =>
					{
						if (Workers.Any(o => _.Value == o))
						{
							AddWorker(_.Value);
						}
						return _;
					});
					ActiveJobs.Clear();
				}
			}
		}
		public void SetStatus(List<JobItem> jobItems, JobStatus jobStatus)
		{
			lock (JobItems) { 
				foreach (var tow in jobItems)
				{
					foreach (var joby in JobItems.ToList())
					{
						if (joby == tow)
						{
							joby.AssignedItems.JobStatus = jobStatus;
						}
					}
				}
			}
		}
		public void DeleteJobs()
		{
			JobItems.RemoveAll(_=>_.AssignedItems.JobStatus == JobStatus.Finished);
		}
		public async Task<object> RunProducer(WorkerType worker,ActionType actionType, IReportHandler reportHandler, string topic, int limit, List<object> items = null, params IServiceRepository[] serviceRepositories)
		{
			var worker_ = GetWorkerByType(worker);
			if (worker_ != null && worker_._worker!=null) {
				try { 
				var results = await worker_.Execute(actionType,reportHandler,topic,limit, items, serviceRepositories);
				if(!(results is bool))
				{
					var results_ = (List<object>) results;
					ActionType[] assignes = new ActionType[] {ActionType.FetchBiography, ActionType.FetchCaptions, ActionType.FetchComments};				
					foreach(var res in results_)
					{
						foreach(var ass in assignes)
						{
							JobItems.Add(new JobItem
							{
								AssignedItems = new Assigned
								{
									Item = res,
									Topic = topic,
									Limit = limit
								},
								AssignedWorker = ass
							});
						}
					}
				}
					AddWorker(worker_);
					return results;
				}
				catch(Exception ee)
				{
					AddWorker(worker_);
				}
			}
			return null;
		}
	}
}
