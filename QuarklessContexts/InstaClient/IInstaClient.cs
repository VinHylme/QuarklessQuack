using InstagramApiSharp.API;
using InstagramApiSharp.Classes;
using QuarklessContexts.Models.InstagramAccounts;
using System.Threading.Tasks;

namespace QuarklessContexts.InstaClient
{
	public interface IInstaClient
	{
		InstaClient Empty();
		IInstaApi ReturnClient {get; }
		InstaClient GetClientFromModel(InstagramClientAccount instagramAccount);
		Task<IResult<InstaLoginResult>> TryLogin(string username, string password);
		Task<IResult<InstaChallengeRequireVerifyMethod>> GetChallengeRequireVerifyMethodAsync(string username, string password);
		Task<IResult<InstaLoginResult>> SubmitChallangeCode(string username, string password, InstaChallengeLoginInfo instaChallengeLoginInfo, string code);
	}
}