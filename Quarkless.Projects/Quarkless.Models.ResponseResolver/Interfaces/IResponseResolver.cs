using System;
using InstagramApiSharp.Classes;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.InstagramClient.Interfaces;
using System.Threading.Tasks;

namespace Quarkless.Models.ResponseResolver.Interfaces
{
	public interface IResponseResolver
	{
		Task<IResult<TInput>> WithResolverAsync<TInput>(Func<Task<IResult<TInput>>> func);
		Task<IResult<TInput>> WithResolverAsync<TInput>(Func<Task<IResult<TInput>>> func,
			ActionType actionType, string request);
		IResponseResolver WithClient(IApiClientContainer client);
		IResponseResolver WithInstaApiClient(IInstaClient client);
		Task<IResult<TInput>> WithResolverAsyncEmpty<TInput>(Func<Task<IResult<TInput>>> func);
		IResponseResolver WithAttempts(int numberOfAttemptsPerRequest = 0, TimeSpan? intervalBetweenRequests = null);
	}
}
