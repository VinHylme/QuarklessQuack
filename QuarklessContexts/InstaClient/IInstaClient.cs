using InstagramApiSharp.API;
using InstagramApiSharp.Classes;
using QuarklessContexts.Models.InstagramAccounts;
using System.Threading.Tasks;
using InstagramApiSharp.Classes.Android.DeviceInfo;
using QuarklessContexts.Models.Proxies;

namespace QuarklessContexts.InstaClient
{
	public interface IInstaClient
	{
		InstaClient Empty();
		IInstaApi ReturnClient {get; }
		InstaClient Empty(ProxyModel proxy, bool genDevice = false);
		InstaClient GetClientFromModel(InstagramClientAccount instagramAccount);
		Task<IResult<string>> TryLogin(string username, string password, AndroidDevice device, ProxyModel proxy = null);
		Task<IResult<InstaChallengeRequireVerifyMethod>> GetChallengeRequireVerifyMethodAsync(string username, string password);
		Task<IResult<InstaLoginResult>> SubmitChallengeCode(string username, string password, InstaChallengeLoginInfo instaChallengeLoginInfo, string code);
	}
}