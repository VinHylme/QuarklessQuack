using InstagramApiSharp.Classes;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.InstagramClient.Interfaces;
using System.Threading.Tasks;

namespace Quarkless.Models.ResponseResolver.Interfaces
{
	public interface IResponseResolver
	{
		Task<IResult<TInput>> WithResolverAsync<TInput>(IResult<TInput> response, ActionType actionType, string request);
		Task<IResult<TInput>> WithResolverAsync<TInput>(IResult<TInput> response);
		IResponseResolver WithClient(IApiClientContainer client);
	}
}
