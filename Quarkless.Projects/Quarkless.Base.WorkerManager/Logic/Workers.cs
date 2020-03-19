using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InstagramApiSharp.Classes;
using Quarkless.Base.InstagramAccounts.Models;
using Quarkless.Base.InstagramAccounts.Models.Enums;
using Quarkless.Base.InstagramAccounts.Models.Interfaces;
using Quarkless.Base.InstagramClient.Models.Interfaces;
using Quarkless.Base.ResponseResolver.Models;
using Quarkless.Base.ResponseResolver.Models.Interfaces;
using Quarkless.Base.WorkerManager.Models;
using Quarkless.Base.WorkerManager.Models.Interfaces;
using Quarkless.Models.Common.Models.Carriers;

namespace Quarkless.Base.WorkerManager.Logic
{
	public class Workers : IWorkers
	{
		#region Init

		/// <summary>
		/// List of instagram worker accounts
		/// </summary>
		private readonly IInstagramAccountLogic _instagramAccountLogic;
		private readonly ConcurrentQueue<IWorker> _workers;
		/// <summary>
		/// This is used during waiting for free workers
		/// </summary>
		private readonly double _waitTime;

		private int currentlyWorking { get; set; }

		/// <summary>
		/// Takes a list of instagram accounts of type 1, and creates a workers list
		/// </summary>
		/// <param name="context"></param>
		/// <param name="instagramAccountLogic"></param>
		/// <param name="workerAccounts"></param>
		/// <param name="waitTime"></param>
		public Workers(IApiClientContext context, IInstagramAccountLogic instagramAccountLogic, 
			IEnumerable<ShortInstagramAccountModel> workerAccounts, double waitTime = 0.35)
		{
			_instagramAccountLogic = instagramAccountLogic;
			_waitTime = waitTime;

			_workers = !workerAccounts.Any() 
				? new ConcurrentQueue<IWorker>()
				: new ConcurrentQueue<IWorker>(workerAccounts.Select(_ => 
					new Worker(context, _instagramAccountLogic, _)));

			NumberOfWorkersTotal = _workers.Count;
		}
		#endregion

		public int NumberOfCurrentlyActiveWorkers => currentlyWorking;
		public bool IsAllOccupied => NumberOfWorkersTotal == NumberOfCurrentlyActiveWorkers;
		public int NumberOfWorkersTotal { get; }

		#region Event Handlers
		public EventHandler<WorkersBatchEventArgs> BatchWorkStarted;
		public EventHandler<WorkersBatchEventArgs> BatchWorkFinished;
		public EventHandler<WorkersBatchFailedEventArgs> BatchWorkFailed;
		public EventHandler<WorkersBatchEventArgs> BatchWorkStopped;

		protected virtual void OnBatchWorkCompleted(WorkersBatchEventArgs e)
		{
			var handler = BatchWorkFinished;
			handler?.Invoke(this,e);
		}
		protected virtual void OnBatchWorkStarted(WorkersBatchEventArgs e)
		{
			var handler = BatchWorkStarted;
			handler?.Invoke(this, e);
		}
		protected virtual void OnBatchWorkStopped(WorkersBatchEventArgs e)
		{
			var handler = BatchWorkStopped;
			handler?.Invoke(this, e);
		}
		protected virtual void OnBatchWorkFailed(WorkersBatchFailedEventArgs e)
		{
			var handler = BatchWorkFailed;
			handler?.Invoke(this, e);
		}
		#endregion

		public async Task PerformBatchTask(Func<Workers, string, Task> action, string topic)
		{
			try
			{
				OnBatchWorkStarted(new WorkersBatchEventArgs
				{
					Workers = this,
					Date = DateTime.UtcNow
				});
				await action(this, topic);
			}
			catch (Exception err)
			{
				OnBatchWorkFailed(new WorkersBatchFailedEventArgs
				{
					Workers = this,
					Error = new ErrorResponse
					{
						Exception = err,
						Message = err.Message
					},
					Date = DateTime.UtcNow
				});
			}
			finally
			{
				OnBatchWorkCompleted(new WorkersBatchEventArgs
				{
					Workers = this,
					Date = DateTime.UtcNow
				});
			}
		}
		public async Task PerformBatchTask(Func<Workers, Task> action)
		{
			try
			{
				OnBatchWorkStarted(new WorkersBatchEventArgs
				{
					Workers = this,
					Date = DateTime.UtcNow
				});
				await action(this);
			}
			catch (Exception err)
			{
				OnBatchWorkFailed(new WorkersBatchFailedEventArgs
				{
					Workers = this,
					Error = new ErrorResponse
					{
						Exception = err,
						Message = err.Message
					},
					Date = DateTime.UtcNow
				});
			}
			finally
			{
				OnBatchWorkCompleted(new WorkersBatchEventArgs
				{
					Workers = this,
					Date = DateTime.UtcNow
				});
			}
		}
		public void PerformBatchTask(Action<Workers> action)
		{
			try
			{
				OnBatchWorkStarted(new WorkersBatchEventArgs
				{
					Workers = this,
					Date = DateTime.UtcNow
				});
				action(this);
			}
			catch (Exception err)
			{
				OnBatchWorkFailed(new WorkersBatchFailedEventArgs
				{
					Workers = this,
					Error = new ErrorResponse
					{
						Exception = err,
						Message = err.Message
					},
					Date = DateTime.UtcNow
				});
			}
			finally
			{
				OnBatchWorkCompleted(new WorkersBatchEventArgs
				{
					Workers = this,
					Date = DateTime.UtcNow
				});
			}
		}

		public async Task<ResolverResponse<TInput>> PerformQueryTaskWithClient<TInput>
			(IResponseResolver responseResolver,Func<IWorker, string, int, Task<IResult<TInput>>> action,
			string query, int limit)
		{
			if (responseResolver == null) throw new Exception("Response Resolver cannot be null");
			var worker = await TakeNextAvailableWorker();
			
			worker.WorkerFinished += (o, e) =>
			{
				AddWorker(e.Worker);
				currentlyWorking--;
			};
			
			worker.WorkerFailed += (o, e) =>
			{
				AddWorker(e.Worker);
				currentlyWorking--;
			};

			worker.WorkerStarted += async (o, e) =>
			{
				await worker.UpdateWorkerState(AgentState.Working);
				currentlyWorking++;
			};

			return await worker.PerformQueryTaskWithWorkerWithClient(responseResolver, action, query, limit);
		}

		public async Task<ResolverResponse<TResult>> PerformQueryTaskWithClient<TResult>
		(IResponseResolver responseResolver, Func<IWorker, object, int, Task<IResult<TResult>>> action,
			object inputObject, int limit)
		{
			if (responseResolver == null) throw new Exception("Response Resolver cannot be null");
			var worker = await TakeNextAvailableWorker();
			
			worker.WorkerFinished += (o, e) =>
			{
				AddWorker(e.Worker);
				currentlyWorking--;
			};
			
			worker.WorkerFailed += (o, e) =>
			{
				AddWorker(e.Worker);
				currentlyWorking--;
			};
			
			worker.WorkerStarted += async (o, e) =>
			{
				await worker.UpdateWorkerState(AgentState.Working);
				currentlyWorking++;
			};

			return await worker.PerformQueryTaskWithWorkerWithClient(responseResolver, action, inputObject, limit);
		}

		public async Task<ResolverResponse<TInput>> PerformQueryTaskWithClient<TInput>
			(IResponseResolver responseResolver, Func<IWorker, int, Task<IResult<TInput>>> action, int limit)
		{
			if (responseResolver == null) throw new Exception("Response Resolver cannot be null");
			var worker = await TakeNextAvailableWorker();
			
			worker.WorkerFinished += (o, e) =>
			{
				AddWorker(e.Worker);
				currentlyWorking--;
			};
			
			worker.WorkerFailed += (o, e) =>
			{
				AddWorker(e.Worker);
				currentlyWorking--;
			};
			
			worker.WorkerStarted += async (o, e) =>
			{
				await worker.UpdateWorkerState(AgentState.Working);
				currentlyWorking++;
			};

			return await worker.PerformQueryTaskWithWorkerWithClient(responseResolver, action, limit);
		}

		public async Task<TResult> PerformQueryTask<TResult>(
			Func<IWorker, string, int, Task<TResult>> action, string query, int limit)
		{
			var worker = await TakeNextAvailableWorker();
			
			worker.WorkerFinished += (o, e) =>
			{
				AddWorker(e.Worker);
				currentlyWorking--;
			};
			
			worker.WorkerFailed += (o, e) =>
			{
				AddWorker(e.Worker);
				currentlyWorking--;
			};
			
			worker.WorkerStarted += async (o, e) =>
			{
				await worker.UpdateWorkerState(AgentState.Working);
				currentlyWorking++;
			};
			
			return await worker.PerformQueryTaskWithWorker(action, query, limit);
		}

		public async Task<TResult> PerformQueryTask<TResult>(
			Func<IWorker, object, int, Task<TResult>> action, object query, int limit)
		{
			var worker = await TakeNextAvailableWorker();
			
			worker.WorkerFinished += (o, e) =>
			{
				AddWorker(e.Worker);
				currentlyWorking--;
			};
			
			worker.WorkerFailed += (o, e) =>
			{
				AddWorker(e.Worker);
				currentlyWorking--;
			};
			
			worker.WorkerStarted += async (o, e) =>
			{
				await worker.UpdateWorkerState(AgentState.Working);
				currentlyWorking++;
			};
			
			return await worker.PerformQueryTaskWithWorker(action, query, limit);
		}

		public async Task<TResult> PerformQueryTask<TResult>(
			Func<IWorker, int, Task<TResult>> action, int limit)
		{
			var worker = await TakeNextAvailableWorker();
			worker.WorkerFinished += (o, e) =>
			{
				AddWorker(e.Worker);
				currentlyWorking--;
			};
			worker.WorkerFailed += (o, e) =>
			{
				AddWorker(e.Worker);
				currentlyWorking--;
			};
			worker.WorkerStarted += (o, e) =>
			{
				currentlyWorking++;
			};
			return await worker.PerformQueryTaskWithWorker(action, limit);
		}

		public TResult PerformAction<TResult>(Func<IWorker, TResult> action)
		{
			var worker = TakeNextAvailableWorker().Result;
			
			worker.WorkerFinished += (o, e) =>
			{
				AddWorker(e.Worker);
				currentlyWorking--;
			};
			
			worker.WorkerFailed += (o, e) =>
			{
				AddWorker(e.Worker);
				currentlyWorking--;
			};
			
			worker.WorkerStarted += async (o, e) =>
			{
				await worker.UpdateWorkerState(AgentState.Working);
				currentlyWorking++;
			};

			return worker.PerformActionWithWorker(action);
		}

		public async Task<TResult> PerformAction<TResult>(
			Func<IWorker, Task<TResult>> action)
		{
			var worker = await TakeNextAvailableWorker();
			worker.WorkerFinished += (o, e) =>
			{
				AddWorker(e.Worker);
				currentlyWorking--;
			};
			worker.WorkerFailed += (o, e) =>
			{
				AddWorker(e.Worker);
				currentlyWorking--;
			};
			worker.WorkerStarted += async (o, e) =>
			{
				await worker.UpdateWorkerState(AgentState.Working);
				currentlyWorking++;
			};

			return await worker.PerformActionWithWorker(action);
		}

		public async void AddWorker(IWorker worker)
		{
			if (_workers.Any(x => x.WorkerUsername == worker.WorkerUsername)) return;
			await worker.UpdateWorkerState(AgentState.NotStarted);
			_workers.Enqueue(worker);
		}

		public async Task<IWorker> TakeNextAvailableWorker()
		{
			if (!_workers.IsEmpty) return !_workers.TryDequeue(out var worker) ? null : worker;

			Console.WriteLine("Waiting for worker to be available...");
			await Task.Delay(TimeSpan.FromSeconds(_waitTime));
			return await TakeNextAvailableWorker();
		}
	}
}