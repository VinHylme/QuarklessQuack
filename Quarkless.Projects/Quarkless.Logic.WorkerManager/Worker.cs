using Quarkless.Logic.InstagramClient;
using Quarkless.Models.InstagramAccounts;
using Quarkless.Models.InstagramAccounts.Interfaces;
using Quarkless.Models.InstagramClient.Interfaces;
using System;
using System.Threading.Tasks;
using Quarkless.Models.Common.Models.Carriers;
using Quarkless.Models.InstagramAccounts.Enums;
using Quarkless.Models.WorkerManager;
using InstagramApiSharp.Classes;
using Quarkless.Models.ResponseResolver.Interfaces;
using Quarkless.Models.WorkerManager.Interfaces;

namespace Quarkless.Logic.WorkerManager
{
	public class Worker : IWorker
	{
		#region Private Properties
		private readonly ShortInstagramAccountModel _workerAccount;
		private readonly IApiClientContext _context;
		private readonly IInstagramAccountLogic _instagramAccountLogic;
		#endregion

		#region Public Properties
		public string WorkerUsername => _workerAccount.Username;
		public string WorkerAccountId => _workerAccount.AccountId;
		public string WorkerInstagramAccountId => _workerAccount.Id;
		public IApiClientContainer Client { get; set; }
		#endregion

		public Worker(IApiClientContext context,
			IInstagramAccountLogic instagramAccountLogic,
			ShortInstagramAccountModel workerAccount)
		{
			_context = context;
			_instagramAccountLogic = instagramAccountLogic;
			_workerAccount = workerAccount;
			Client = new ApiClientContainer(context, _workerAccount.AccountId, _workerAccount.Id);
		}

		public Worker(IApiClientContext context, IInstagramAccountLogic instagramAccountLogic, string accountId,
			string instagramAccountId)
		{
			_context = context;
			_instagramAccountLogic = instagramAccountLogic;
			Client = new ApiClientContainer(_context, accountId, instagramAccountId);
			_workerAccount = Client.GetContext.InstagramAccount;
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
			=> Client = new ApiClientContainer(_context, accountId, instagramAccountId);

		public async Task UpdateWorkerState(AgentState state)
		{
			await _instagramAccountLogic.PartialUpdateInstagramAccount(WorkerAccountId,
				WorkerInstagramAccountId, new InstagramAccountModel
				{
					AgentState = (int)state
				});
		}

		public TResult PerformActionWithWorker<TResult>(Func<IWorker, TResult> action)
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
				return default;
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
			Func<IWorker, Task<TResult>> action)
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
				return default;
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
			Func<IWorker, string, int, Task<TResult>> action, string query, int limit)
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
				return default;
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
			Func<IWorker, int, Task<TResult>> action, int limit)
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
				return default;
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
			Func<IWorker, object, int, Task<TResult>> action, object inputObject, int limit)
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
				return default;
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
			Func<IWorker, string, int, Task<IResult<TInput>>> action, string query, int limit)
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
			Func<IWorker, int, Task<IResult<TInput>>> action, int limit)
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
			Func<IWorker, object, int, Task<IResult<TInput>>> action, object inputObject, int limit)
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