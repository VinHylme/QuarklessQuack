using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InstagramApiSharp.Classes;
using QuarklessContexts.Classes.Carriers;
using QuarklessContexts.Models.InstagramAccounts;
using QuarklessLogic.Handlers.ClientProvider;
using QuarklessLogic.Logic.ResponseLogic;

namespace QuarklessLogic.Handlers.WorkerManagerService
{
	public class Workers : IWorkers
	{
		#region Init
		/// <summary>
		/// List of instagram worker accounts
		/// </summary>
		private readonly ConcurrentQueue<Worker> _workers;

		public EventHandler<WorkersBatchEventArgs> BatchWorkStarted;
		public EventHandler<WorkersBatchEventArgs> BatchWorkFinished;
		public EventHandler<WorkersBatchFailedEventArgs> BatchWorkFailed;

		/// <summary>
		/// This is used during waiting for free workers
		/// </summary>
		private readonly double _waitTime;
		private readonly int _initialSize;
		private int currentlyWorking { get; set; }

		/// <summary>
		/// Takes a list of instagram accounts of type 1, and creates a workers list
		/// </summary>
		/// <param name="context"></param>
		/// <param name="workerAccounts"></param>
		/// <param name="waitTime"></param>
		public Workers(IAPIClientContext context, IEnumerable<ShortInstagramAccountModel> workerAccounts, 
			double waitTime = 0.35)
		{
			_workers = new ConcurrentQueue<Worker>(workerAccounts.Select(_ => new Worker(context,_)));
			_initialSize = _workers.Count;
			_waitTime = waitTime;
		}
		#endregion

		public int NumberOfCurrentlyActiveWorkers => currentlyWorking;
		public bool IsAllOccupied => _initialSize == NumberOfCurrentlyActiveWorkers;

		public void AddWorker(Worker worker)
		{
			if (_workers.Contains(worker)) return;
			_workers.Enqueue(worker);
		}

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
		protected virtual void OnBatchWorkFailed(WorkersBatchFailedEventArgs e)
		{
			var handler = BatchWorkFailed;
			handler?.Invoke(this, e);
		}

		public async Task PerformBatchTask(Func<Workers, string, Task> action, string topic)
		{
			try
			{
				OnBatchWorkStarted(new WorkersBatchEventArgs
				{
					Workers = _workers,
					Date = DateTime.UtcNow
				});
				await action(this, topic);
			}
			catch (Exception err)
			{
				OnBatchWorkFailed(new WorkersBatchFailedEventArgs
				{
					Workers = _workers,
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
					Workers = _workers,
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
					Workers = _workers,
					Date = DateTime.UtcNow
				});
				await action(this);
			}
			catch (Exception err)
			{
				OnBatchWorkFailed(new WorkersBatchFailedEventArgs
				{
					Workers = _workers,
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
					Workers = _workers,
					Date = DateTime.UtcNow
				});
			}
		}

		public async Task<IResult<TInput>> PerformQueryTaskWithClient<TInput>
			(IResponseResolver responseResolver,Func<Worker, string, int, Task<IResult<TInput>>> action,
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
			worker.WorkerStarted += (o, e) =>
			{
				currentlyWorking++;
			};

			return await responseResolver.WithClient(worker.Client)
				.WithResolverAsync(await action(worker, query, limit));
		}

		public async Task<IResult<TInput>> PerformQueryTaskWithClient<TInput>
			(IResponseResolver responseResolver, Func<Worker, int, Task<IResult<TInput>>> action, int limit)
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
			worker.WorkerStarted += (o, e) =>
			{
				currentlyWorking++;
			};

			return await responseResolver.WithClient(worker.Client)
				.WithResolverAsync(await action(worker, limit));
		}

		public async Task<TResult> PerformQueryTask<TResult>(
			Func<Worker, string, int, Task<TResult>> action, string query, int limit)
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
			return await worker.PerformQueryTaskWithWorker(action, query, limit);
		}

		public async Task<TResult> PerformQueryTask<TResult>(
			Func<Worker, int, Task<TResult>> action, int limit)
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

		public async Task<TResult> PerformAction<TResult>(Func<Worker, TResult> action)
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

			return worker.PerformActionWithWorker(action);
		}

		public async Task<TResult> PerformAction<TResult>(
			Func<Worker, Task<TResult>> action)
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

			return await worker.PerformActionWithWorker(action);
		}

		public async Task<Worker> TakeNextAvailableWorker()
		{
			while (_workers.IsEmpty)
				await Task.Delay(TimeSpan.FromSeconds(_waitTime));
			return !_workers.TryDequeue(out var worker) ? null : worker;
		}
	}
}