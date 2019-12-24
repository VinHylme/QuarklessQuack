using System;
using System.Threading.Tasks;
using InstagramApiSharp.Classes;
using QuarklessContexts.Classes.Carriers;
using QuarklessContexts.Models.InstagramAccounts;
using QuarklessLogic.Handlers.ClientProvider;
using QuarklessLogic.Logic.InstagramAccountLogic;
using QuarklessLogic.Logic.ResponseLogic;

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
		private readonly IAPIClientContext _context;
		private readonly IInstagramAccountLogic _instagramAccountLogic;
		#endregion

		#region Public Properties
		public string WorkerUsername => _workerAccount.Username;
		public string WorkerAccountId => _workerAccount.AccountId;
		public string WorkerInstagramAccountId => _workerAccount.Id;
		public IAPIClientContainer Client { get; set; }
		#endregion

		public Worker(IAPIClientContext context, 
			IInstagramAccountLogic instagramAccountLogic, 
			ShortInstagramAccountModel workerAccount)
		{
			_context = context;
			_instagramAccountLogic = instagramAccountLogic;
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

		public void ChangeUser(string accountId, string instagramAccountId)
			=> Client = new APIClientContainer(_context, accountId, instagramAccountId);

		public async Task UpdateWorkerState(AgentState state)
		{
			await _instagramAccountLogic.PartialUpdateInstagramAccount(WorkerAccountId,
				WorkerInstagramAccountId, new InstagramAccountModel
				{
					AgentState = (int)state
				});
		}

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
		public async Task<TResult> PerformQueryTaskWithWorker<TResult>(
			Func<Worker, object, int, Task<TResult>> action, object inputObject, int limit)
		{
			try
			{
				OnTaskStarted(new WorkerEventArgs
				{
					Worker = this,
					Date = DateTime.UtcNow
				});
				var result = await action(this, inputObject, limit);
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

		public async Task<IResult<TInput>> PerformQueryTaskWithWorkerWithClient<TInput>(
			IResponseResolver responseResolver,
			Func<Worker, string, int, Task<IResult<TInput>>> action, string query, int limit)
		{
			try
			{
				OnTaskStarted(new WorkerEventArgs
				{
					Worker = this,
					Date = DateTime.UtcNow
				});
				var result = await responseResolver.WithClient(Client)
					.WithResolverAsync(await action(this, query, limit));
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
				return null;
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

		public async Task<IResult<TInput>> PerformQueryTaskWithWorkerWithClient<TInput>(
			IResponseResolver responseResolver,
			Func<Worker, int, Task<IResult<TInput>>> action, int limit)
		{
			try
			{
				OnTaskStarted(new WorkerEventArgs
				{
					Worker = this,
					Date = DateTime.UtcNow
				});
				var result = await responseResolver.WithClient(Client)
					.WithResolverAsync(await action(this, limit));
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
				return null;
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
		public async Task<IResult<TInput>> PerformQueryTaskWithWorkerWithClient<TInput>(
			IResponseResolver responseResolver,
			Func<Worker, object, int, Task<IResult<TInput>>> action, object inputObject, int limit)
		{
			try
			{
				OnTaskStarted(new WorkerEventArgs
				{
					Worker = this,
					Date = DateTime.UtcNow
				});
				var result = await responseResolver.WithClient(Client)
					.WithResolverAsync(await action(this, inputObject, limit));
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
				return null;
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