﻿using System;
using System.Threading.Tasks;
using QuarklessContexts.Classes.Carriers;
using QuarklessContexts.Models.InstagramAccounts;
using QuarklessLogic.Handlers.ClientProvider;

namespace QuarklessLogic.Handlers.WorkerManagerService
{
	public class WorkerEventArgs : EventArgs
	{
		public Worker Worker { get; set; }
		public DateTime Date { get; set; }
	}
	public class WorkerFailedEventArgs : EventArgs
	{
		public Worker Worker { get; set; }
		public DateTime Date { get; set; }
		public ErrorResponse Error { get; set; }
	}
	public class Worker
	{
		#region Private Properties
		private readonly ShortInstagramAccountModel _workerAccount;
		#endregion

		#region Public Properties
		public string WorkerUsername => _workerAccount.Username;
		public IAPIClientContainer Client { get; set; }
		#endregion

		public Worker(IAPIClientContext context, ShortInstagramAccountModel workerAccount)
		{
			_workerAccount = workerAccount;
			Client = new APIClientContainer(context, _workerAccount.AccountId, _workerAccount.Id);
		}
		
		#region Event Handlers
		public event EventHandler<WorkerEventArgs> WorkerFinished;
		public event EventHandler<WorkerEventArgs> WorkerStarted;
		public event EventHandler<WorkerFailedEventArgs> WorkerFailed;

		protected virtual void OnTaskCompleted(WorkerEventArgs e)
		{
			var handler = WorkerFinished;
			handler?.Invoke(this, e);
		}
		protected virtual void OnTaskStarted(WorkerEventArgs e)
		{
			var handler = WorkerStarted;
			handler?.Invoke(this, e);
		}
		protected virtual void OnTaskFailed(WorkerFailedEventArgs e)
		{
			var handler = WorkerFailed;
			handler?.Invoke(this, e);
		}
		#endregion

		public TResult PerformActionWithWorker<TResult>(Func<Worker, TResult> action)
		{
			try
			{
				OnTaskStarted(new WorkerEventArgs
				{
					Worker = this,
					Date = DateTime.UtcNow
				});
				return action(this);
			}
			catch (Exception err)
			{
				OnTaskFailed(new WorkerFailedEventArgs
				{
					Date = DateTime.UtcNow,
					Error = new ErrorResponse
					{
						Exception = err,
						Message = err.Message,
					},
					Worker = this
				});
				return default(TResult);
			}
			finally
			{
				OnTaskCompleted(new WorkerEventArgs
				{
					Worker = this,
					Date = DateTime.UtcNow
				});
			}
		}

		public async Task<TResult> PerformActionWithWorker<TResult>(
			Func<Worker, Task<TResult>> action)
		{
			try
			{
				OnTaskStarted(new WorkerEventArgs
				{
					Worker = this,
					Date = DateTime.UtcNow
				});
				return await action(this);
			}
			catch (Exception err)
			{
				OnTaskFailed(new WorkerFailedEventArgs
				{
					Date = DateTime.UtcNow,
					Error = new ErrorResponse
					{
						Exception = err,
						Message = err.Message,
					},
					Worker = this
				});
				Console.WriteLine(err.Message);
				return default(TResult);
			}
			finally
			{
				OnTaskCompleted(new WorkerEventArgs
				{
					Worker = this,
					Date = DateTime.UtcNow
				});
			}
		}

		public async Task<TResult> PerformQueryTaskWithWorker<TResult>(
			Func<Worker, string, int, Task<TResult>> action, string query, int limit)
		{
			try
			{
				OnTaskStarted(new WorkerEventArgs
				{
					Worker = this,
					Date = DateTime.UtcNow
				});
				var result = await action(this, query, limit);
				return result;
			}
			catch (Exception err)
			{
				OnTaskFailed(new WorkerFailedEventArgs
				{
					Date = DateTime.UtcNow,
					Error = new ErrorResponse
					{
						Exception = err,
						Message = err.Message,
					},
					Worker = this
				});
				Console.WriteLine(err.Message);
				return default(TResult);
			}
			finally
			{
				OnTaskCompleted(new WorkerEventArgs
				{
					Worker = this,
					Date = DateTime.UtcNow
				});
			}
		}

		public async Task<TResult> PerformQueryTaskWithWorker<TResult>(
			Func<Worker, int, Task<TResult>> action, int limit)
		{
			try
			{
				OnTaskStarted(new WorkerEventArgs
				{
					Worker = this,
					Date = DateTime.UtcNow
				});
				var result = await action(this, limit);
				return result;
			}
			catch (Exception err)
			{
				OnTaskFailed(new WorkerFailedEventArgs
				{
					Date = DateTime.UtcNow,
					Error = new ErrorResponse
					{
						Exception = err,
						Message = err.Message,
					},
					Worker = this
				});
				return default(TResult);
			}
			finally
			{
				OnTaskCompleted(new WorkerEventArgs
				{
					Worker = this,
					Date = DateTime.UtcNow
				});
			}
		}
	}
}