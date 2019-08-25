using System.Threading.Tasks;
using InstagramApiSharp.Classes;
using QuarklessContexts.Enums;
using QuarklessLogic.Handlers.ClientProvider;

namespace QuarklessLogic.Logic.ResponseLogic
{
	public interface IResponseResolver
	{
		Task<IResult<TInput>> WithResolverAsync<TInput>(IResult<TInput> response, ActionType actionType, string request);
		Task<IResult<TInput>> WithResolverAsync<TInput>(IResult<TInput> response);
		IResponseResolver WithClient(IAPIClientContainer client);
	}
}