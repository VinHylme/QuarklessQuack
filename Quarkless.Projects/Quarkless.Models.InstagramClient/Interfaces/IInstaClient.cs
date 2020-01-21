using System.Threading.Tasks;
using InstagramApiSharp.API;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Android.DeviceInfo;
using Quarkless.Models.Proxy;

namespace Quarkless.Models.InstagramClient.Interfaces
{
	public interface IInstaClient
	{
		IInstaClient Empty();
		IInstaApi ReturnClient {get; }
		IInstaClient StateClient(string state);
		Task<string> GetStateDataFromString();
		void LoadStateDataFromStringAsync(string state);
		IInstaClient Empty(ProxyModel proxy, bool genDevice = false);
		IInstaClient GetClientFromModel(InstagramClientAccount instagramAccount);
		Task<IResult<string>> TryLogin(string username, string password, AndroidDevice device, ProxyModel proxy = null);
		Task<IResult<InstaChallengeRequireVerifyMethod>> GetChallengeRequireVerifyMethodAsync(string username, string password);
		Task<IResult<InstaLoginResult>> SubmitChallengeCode(string username, string password, InstaChallengeLoginInfo instaChallengeLoginInfo, string code);
	}
}