using System;
using System.Threading.Tasks;
using InstagramApiSharp.Classes;
using QuarklessLogic.Logic.ResponseLogic;

namespace QuarklessLogic.Handlers.WorkerManagerService
{
	public interface IWorkers
	{
		int NumberOfCurrentlyActiveWorkers { get; }
		bool IsAllOccupied { get; }
		void AddWorker(Worker worker);
		//Task<Worker> TakeNextAvailableWorker();
		Task<TResult> PerformQueryTask<TResult>(
			Func<Worker, string, int, Task<TResult>> action, string query, int limit);

		Task<TResult> PerformQueryTask<TResult>(
			Func<Worker, int, Task<TResult>> action, int limit);

		TResult PerformAction<TResult>(Func<Worker, TResult> action);

		Task<TResult> PerformAction<TResult>(
			Func<Worker, Task<TResult>> action);

		Task<IResult<TInput>> PerformQueryTaskWithClient<TInput>
			(IResponseResolver responseResolver, Func<Worker, string, int, Task<IResult<TInput>>> action, 
			string query, int limit);

		Task<TResult> PerformQueryTask<TResult>(
			Func<Worker, object, int, Task<TResult>> action, object query, int limit);
		Task<IResult<TInput>> PerformQueryTaskWithClient<TInput>
			(IResponseResolver responseResolver, Func<Worker, int, Task<IResult<TInput>>> action, int limit);

		Task<IResult<TResult>> PerformQueryTaskWithClient<TResult>
		(IResponseResolver responseResolver, Func<Worker, object, int, Task<IResult<TResult>>> action,
			object inputObject, int limit);
	}
}