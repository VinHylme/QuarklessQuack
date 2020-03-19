using System;
using System.Threading.Tasks;
using InstagramApiSharp.Classes;
using Quarkless.Base.InstagramAccounts.Models;
using Quarkless.Base.InstagramClient.Models.Interfaces;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Common.Interfaces;

namespace Quarkless.Base.ResponseResolver.Models.Interfaces
{
	public interface IResponseResolver
	{
		Task CheckBlockStates(ShortInstagramAccountModel account);
		Task<ResolverResponse<TInput>> WithResolverAsync<TInput>(Func<Task<IResult<TInput>>> func);
		Task<ResolverResponse<TInput>> WithResolverAsync<TInput>(Func<Task<IResult<TInput>>> func,
			ActionType actionType, IExec request);

		/// <summary>
		/// You should not be using with attempts on this function
		/// </summary>
		/// <typeparam name="TInput"></typeparam>
		/// <param name="response"></param>
		/// <returns></returns>
		Task<ResolverResponse<TInput>> WithResolverAsync<TInput>(IResult<TInput> response);
		IResponseResolver WithClient(IApiClientContainer client);
		IResponseResolver WithInstaApiClient(IInstaClient client);
		IResponseResolver WithAttempts(int numberOfAttemptsPerRequest = 0, TimeSpan? intervalBetweenRequests = null);
	}
}
