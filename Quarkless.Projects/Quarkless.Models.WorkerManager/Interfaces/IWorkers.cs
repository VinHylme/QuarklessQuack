using System;
using System.Threading.Tasks;
using InstagramApiSharp.Classes;
using Quarkless.Models.ResponseResolver.Interfaces;

namespace Quarkless.Models.WorkerManager.Interfaces
{
	public interface IWorkers
	{
		int NumberOfCurrentlyActiveWorkers { get; }
		bool IsAllOccupied { get; }
		void AddWorker(IWorker worker);

		//Task<Worker> TakeNextAvailableWorker();

		Task<TResult> PerformQueryTask<TResult>(
			Func<IWorker, string, int, Task<TResult>> action, string query, int limit);

		Task<TResult> PerformQueryTask<TResult>(
			Func<IWorker, int, Task<TResult>> action, int limit);

		TResult PerformAction<TResult>(Func<IWorker, TResult> action);

		Task<TResult> PerformAction<TResult>(
			Func<IWorker, Task<TResult>> action);

		Task<IResult<TInput>> PerformQueryTaskWithClient<TInput>
			(IResponseResolver responseResolver, Func<IWorker, string, int, Task<IResult<TInput>>> action, 
			string query, int limit);

		Task<TResult> PerformQueryTask<TResult>(
			Func<IWorker, object, int, Task<TResult>> action, object query, int limit);
		Task<IResult<TInput>> PerformQueryTaskWithClient<TInput>
			(IResponseResolver responseResolver, Func<IWorker, int, Task<IResult<TInput>>> action, int limit);

		Task<IResult<TResult>> PerformQueryTaskWithClient<TResult>
		(IResponseResolver responseResolver, Func<IWorker, object, int, Task<IResult<TResult>>> action,
			object inputObject, int limit);
	}
}