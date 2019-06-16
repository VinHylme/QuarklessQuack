using System.Threading.Tasks;

namespace Quarkless.Services
{
	public interface IAgentServicer
	{
		Task<bool> Start(string user, string instauser, string accessToken);
		string GetName { get; }
		IAgentServicer Create(string userId, string instaId);
	}
}