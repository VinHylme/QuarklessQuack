using InstagramApiSharp.API;
using QuarklessContexts.Models.InstagramAccounts;
using System.Threading.Tasks;

namespace QuarklessContexts.InstaClient
{
	public interface IInstaClient
	{
		InstaClient Empty();
		IInstaApi ReturnClient {get; }
		InstaClient GetClientFromModel(InstagramClientAccount instagramAccount);
		Task<bool> TryLogin(string username, string password);
	}
}