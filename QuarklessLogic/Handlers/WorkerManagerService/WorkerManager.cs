using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MoreLinq;
using QuarklessContexts.Models.InstagramAccounts;
using QuarklessLogic.Handlers.ClientProvider;
using QuarklessLogic.Logic.InstagramAccountLogic;

namespace QuarklessLogic.Handlers.WorkerManagerService
{
	public class WorkerManagerUpdateEventArgs : EventArgs
	{
		public int TotalWorkerCount { get; set; }
		public WorkerManager Instance { get; set; }
		public DateTime UpdateTime { get; set; }
	}

	public class WorkerManager : IWorkerManager
	{
		#region Private Declare
		private Timer _timer;
		private ConcurrentQueue<Workers> _batchWorkers;
		private readonly int _updateLoopTime;
		private readonly IAPIClientContext _context;
		private readonly IInstagramAccountLogic _instagramAccountLogic;
		private int _batchSize;
		private int _workerAccountType;
		private int _accountWorkersFetched;
		#endregion

		#region Init
		public WorkerManager(IAPIClientContext context, IInstagramAccountLogic instagramAccountLogic, 
			int batchSize = 1, int workerAccountType = 1)
		{
			_context = context;
			_instagramAccountLogic = instagramAccountLogic;
			_batchSize = batchSize;
			_workerAccountType = workerAccountType;
			_updateLoopTime = (int)TimeSpan.FromMinutes(8).TotalMilliseconds;
			_batchWorkers = SetBatchWorkers();
			_timer = SetWorkerPoolTimer();
		}
		#endregion

		#region Worker & Timer Setters
		private Timer SetWorkerPoolTimer()
		{
			return new Timer(_=>OnCallBack(), null, _updateLoopTime, Timeout.Infinite);
		}
		private ConcurrentQueue<Workers> SetBatchWorkers()
		{
			var accounts = _instagramAccountLogic.GetInstagramAccounts(_workerAccountType).Result
				?.Where(_=>_.AgentState != (int) AgentState.Working)
				.Batch(_batchSize)
				.ToList();

			if(accounts == null || !accounts.Any()) return new ConcurrentQueue<Workers>();
			_accountWorkersFetched = accounts.Count;
			return new ConcurrentQueue<Workers>(accounts.Select(_ => new Workers(_context,_instagramAccountLogic, _)));
		}
		private void OnCallBack()
		{
			_timer.Dispose();
			OnBatchWorkUpdateStarting(new WorkerManagerUpdateEventArgs
			{
				TotalWorkerCount = _accountWorkersFetched,
				Instance = this,
				UpdateTime = DateTime.UtcNow
			});
			Task.Delay(TimeSpan.FromSeconds(1)).GetAwaiter().GetResult();
			_batchWorkers = SetBatchWorkers();
			OnBatchWorkFinishedUpdating(new WorkerManagerUpdateEventArgs
			{
				TotalWorkerCount = _accountWorkersFetched,
				Instance = this,
				UpdateTime = DateTime.UtcNow
			});
			_timer = SetWorkerPoolTimer();
		}
		#endregion

		#region Event Handlers
		public event EventHandler<WorkerManagerUpdateEventArgs> WorkerManagerUpdateStarting;
		public event EventHandler<WorkerManagerUpdateEventArgs> WorkerManagerFinishedUpdating;
		protected virtual void OnBatchWorkUpdateStarting(WorkerManagerUpdateEventArgs args)
		{
			var handler = WorkerManagerUpdateStarting;
			handler?.Invoke(this, args);
		}
		protected virtual void OnBatchWorkFinishedUpdating(WorkerManagerUpdateEventArgs args)
		{
			var handler = WorkerManagerFinishedUpdating;
			handler?.Invoke(this, args);
		}
		#endregion

		public async Task PerformTaskOnWorkers(Func<IWorkers, Task> action)
		{
			var workers = await TakeNextAvailableWorkers();

			workers.BatchWorkStarted += (o, e) =>
			{
				
			};

			workers.BatchWorkFinished += (o, e) =>
			{
				if (_accountWorkersFetched == _batchWorkers.Sum(x=>x.NumberOfWorkersTotal)) return;
				_batchWorkers.Enqueue(workers);
			};

			workers.BatchWorkFailed += (o, e) =>
			{
				Console.WriteLine(e.Error.Message);
				_batchWorkers.Enqueue(workers);
			};

			await workers.PerformBatchTask(action);
		}

		public async Task PerformTaskOnWorkers(Action<IWorkers> action)
		{
			var workers = await TakeNextAvailableWorkers();

			workers.BatchWorkStarted += (o, e) =>
			{

			};

			workers.BatchWorkFinished += (o, e) =>
			{
				if (_accountWorkersFetched == _batchWorkers.Sum(x => x.NumberOfWorkersTotal)) return;
				_batchWorkers.Enqueue(workers);
			};

			workers.BatchWorkFailed += (o, e) =>
			{
				Console.WriteLine(e.Error.Message);
				_batchWorkers.Enqueue(workers);
			};

			workers.PerformBatchTask(action);
		}

		public async Task PerformTaskOnWorkers(Func<IWorkers, string, Task> action, string topic)
		{
			var workers = await TakeNextAvailableWorkers();
			workers.BatchWorkStarted += (o, e) =>
			{
			};
			workers.BatchWorkFinished += (o, e) =>
			{
				if (_accountWorkersFetched == _batchWorkers.Sum(x => x.NumberOfWorkersTotal)) return;
				_batchWorkers.Enqueue(workers);
			};
			workers.BatchWorkFailed += (o, e) =>
			{
				Console.WriteLine(e.Error.Message);
				_batchWorkers.Enqueue(workers);
			};
			await workers.PerformBatchTask(action, topic);
		}

		protected async Task<Workers> TakeNextAvailableWorkers()
		{
			_batchWorkers = SetBatchWorkers();
			if (!_batchWorkers.IsEmpty) return !_batchWorkers.TryDequeue(out var workers) ? null : workers;

			Console.WriteLine("Waiting for workers to be available...");
			await Task.Delay(TimeSpan.FromSeconds(0.35));
			return await TakeNextAvailableWorkers();
		}
	}
}
