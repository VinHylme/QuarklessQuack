using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InstagramApiSharp.Classes;
using MoreLinq.Extensions;
using Newtonsoft.Json;
using Quarkless.Logic.InstagramClient;
using Quarkless.Models.InstagramAccounts;
using Quarkless.Models.InstagramAccounts.Enums;
using Quarkless.Models.InstagramAccounts.Interfaces;
using Quarkless.Models.InstagramClient.Interfaces;
using Quarkless.Models.ResponseResolver.Interfaces;
using Quarkless.Models.WorkerManager;
using Quarkless.Models.WorkerManager.Interfaces;

namespace Quarkless.Logic.WorkerManager
{
	public class WorkerManager : IWorkerManager
	{
		#region Private Declare
		private Timer _timer;
		private Timer _refreshTimer;
		private ConcurrentQueue<Workers> _batchWorkers;
		private readonly int _updateLoopTime;
		private readonly int _refreshLoopTime;
		private readonly IApiClientContext _context;
		private readonly IInstagramAccountLogic _instagramAccountLogic;
		private readonly IResponseResolver _responseResolver;
		private int _batchSize;
		private int _workerAccountType;
		private int _accountWorkersFetched;
		#endregion

		#region Init
		public WorkerManager(IApiClientContext context, IInstagramAccountLogic instagramAccountLogic, 
			IResponseResolver responseResolver, int batchSize = 1, int workerAccountType = 1)
		{
			_context = context;
			_instagramAccountLogic = instagramAccountLogic;
			_responseResolver = responseResolver;
			_batchSize = batchSize;
			_workerAccountType = workerAccountType;
			_updateLoopTime = (int)TimeSpan.FromMinutes(8).TotalMilliseconds;
			_refreshLoopTime = (int) TimeSpan.FromMinutes(30).TotalMilliseconds;
			RefreshAccounts().GetAwaiter().GetResult();
			_batchWorkers = SetBatchWorkers();
			_timer = SetWorkerPoolTimer();
		}
		#endregion

		#region Worker & Timer Setters
		private Timer SetWorkerPoolTimer()
		{
			return new Timer(_=> OnCallBack(), null, _updateLoopTime, Timeout.Infinite);
		}
		private Timer SetRefreshAccountsTimer()
		{
			return new Timer(async _=> await OnRefreshCallback(), null, _refreshLoopTime, Timeout.Infinite);
		}
		private async Task OnRefreshCallback()
		{
			_refreshTimer.Dispose();
			await RefreshAccounts();
			OnCallBack();
			_refreshTimer = SetRefreshAccountsTimer();
		}

		private async Task RefreshAccounts()
		{
			var workerAccountsTotal = _instagramAccountLogic.GetInstagramAccountsFull(_workerAccountType).Result;

			foreach (var accountModel in workerAccountsTotal)
			{
				if(accountModel.AgentState == null) 
					continue;

				switch ((AgentState)accountModel.AgentState)
				{
					case AgentState.Blocked:
						break;
					case AgentState.AwaitingActionFromUser:
						break;
					case AgentState.DeepSleep:
						if (accountModel.SleepTimeRemaining == null)
						{
							await _instagramAccountLogic.PartialUpdateInstagramAccount(
								accountModel.AccountId, accountModel._id,
								new InstagramAccountModel
								{
									AgentState = (int)AgentState.NotStarted
								});
						}
						else if (DateTime.UtcNow > accountModel.SleepTimeRemaining.Value)
						{
							await _instagramAccountLogic.PartialUpdateInstagramAccount(
								accountModel.AccountId, accountModel._id,
								new InstagramAccountModel
								{
									AgentState = (int) AgentState.NotStarted
								});
						}
						break;
					case AgentState.Working:
						continue;
					case AgentState.Running:
						continue;
				}
			}
		}
		private ConcurrentQueue<Workers> SetBatchWorkers()
		{
			var accounts = _instagramAccountLogic.GetInstagramAccounts(_workerAccountType).Result
				?.Where(_=>_.AgentState != (int) AgentState.Working && _.AgentState != (int) AgentState.Blocked)
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
