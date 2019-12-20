using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
		public ConcurrentQueue<Workers> BatchWorkers { get; set; }
		public DateTime UpdateTime { get; set; }

	}
	public class WorkerManager
	{
		protected event EventHandler<WorkerManagerUpdateEventArgs> UpdateWorkers;
		private readonly ConcurrentQueue<Workers> _batchWorkers;
		private Timer timer;
		private readonly int updateLoopTime = TimeSpan.FromSeconds(10).Milliseconds;
		#region Init
		public WorkerManager(IAPIClientContext context, IInstagramAccountLogic instagramAccountLogic, 
			int batchSize = 1, int workerAccountType = 1)
		{
			var accounts = instagramAccountLogic.GetInstagramAccounts(workerAccountType).Result.Batch(batchSize);
			_batchWorkers = new ConcurrentQueue<Workers>(accounts.Select(_ => new Workers(context,_)));
			UpdateWorkerPool();
		}
		public WorkerManager(IAPIClientContext context, IEnumerable<ShortInstagramAccountModel> workerAccounts, int batchSize = 1)
		{
			var accounts = workerAccounts.Batch(batchSize);
			_batchWorkers = new ConcurrentQueue<Workers>(accounts.Select(_ => new Workers(context, _)));
			UpdateWorkerPool();
		}
		#endregion

		protected void UpdateWorkerPool()
		{
			timer = new Timer(OnCallBack, this, updateLoopTime, Timeout.Infinite);
		}
		private void OnCallBack(object state)
		{
			timer.Dispose();
			
			timer = new Timer(OnCallBack, this, updateLoopTime, Timeout.Infinite);
		}
		protected virtual void OnBatchWorkUpdating(WorkerManagerUpdateEventArgs args)
		{
			var handler = UpdateWorkers;
			handler?.Invoke(this, args);
		}
		public async Task PerformTaskOnWorkers(Func<IWorkers, Task> action)
		{
			var workers = await TakeNextAvailableWorkers();
			workers.BatchWorkStarted += (o, e) =>
			{

			};
			workers.BatchWorkFinished += (o, e) =>
			{
				_batchWorkers.Enqueue(workers);
			};
			workers.BatchWorkFailed += (o, e) =>
			{
				_batchWorkers.Enqueue(workers);
			};
			await workers.PerformBatchTask(action);
		}
		public async Task PerformTaskOnWorkers(Func<IWorkers, string, Task> action, string topic)
		{
			var workers = await TakeNextAvailableWorkers();
			workers.BatchWorkStarted += (o, e) =>
			{
				
			};
			workers.BatchWorkFinished += (o, e) =>
			{
				_batchWorkers.Enqueue(workers);
			};
			workers.BatchWorkFailed += (o, e) =>
			{
				_batchWorkers.Enqueue(workers);
			};
			await workers.PerformBatchTask(action, topic);
		}

		protected async Task<Workers> TakeNextAvailableWorkers()
		{
			while (_batchWorkers.IsEmpty)
				await Task.Delay(TimeSpan.FromSeconds(0.34));
			return !_batchWorkers.TryDequeue(out var worker) ? null : worker;
		}
	}
}
