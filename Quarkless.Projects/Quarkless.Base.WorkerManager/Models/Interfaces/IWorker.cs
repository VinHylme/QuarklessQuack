﻿using System;
using System.Threading.Tasks;
using InstagramApiSharp.Classes;
using Quarkless.Base.InstagramAccounts.Models.Enums;
using Quarkless.Base.InstagramClient.Models.Interfaces;
using Quarkless.Base.ResponseResolver.Models;
using Quarkless.Base.ResponseResolver.Models.Interfaces;

namespace Quarkless.Base.WorkerManager.Models.Interfaces
{
	public interface IWorker
	{
		event EventHandler<WorkerEventArgs> WorkerFinished;
		event EventHandler<WorkerEventArgs> WorkerStarted;
		event EventHandler<WorkerFailedEventArgs> WorkerFailed;
		IApiClientContainer Client { get; set; }
		string WorkerUsername { get; }
		string WorkerAccountId { get; }
		string WorkerInstagramAccountId { get; }
		void ChangeUser(string accountId, string instagramAccountId);
		Task UpdateWorkerState(AgentState state);
		TResult PerformActionWithWorker<TResult>(Func<IWorker, TResult> action);
		Task<TResult> PerformActionWithWorker<TResult>(Func<IWorker, Task<TResult>> action);
		Task<TResult> PerformQueryTaskWithWorker<TResult>(Func<IWorker, string, int, Task<TResult>> action,
			string query, int limit);
		Task<TResult> PerformQueryTaskWithWorker<TResult>(Func<IWorker, int, Task<TResult>> action, int limit);

		Task<TResult> PerformQueryTaskWithWorker<TResult>(
			Func<IWorker, object, int, Task<TResult>> action, object inputObject, int limit);

		Task<ResolverResponse<TInput>> PerformQueryTaskWithWorkerWithClient<TInput>(IResponseResolver responseResolver, 
			Func<IWorker, string, int, Task<IResult<TInput>>> action, string query, int limit);

		Task<ResolverResponse<TInput>> PerformQueryTaskWithWorkerWithClient<TInput>(IResponseResolver responseResolver,
			Func<IWorker, int, Task<IResult<TInput>>> action, int limit);

		Task<ResolverResponse<TInput>> PerformQueryTaskWithWorkerWithClient<TInput>(IResponseResolver responseResolver,
			Func<IWorker, object, int, Task<IResult<TInput>>> action, object inputObject, int limit);
	}
}
