using System.Threading.Tasks;

namespace Quarkless.Services
{
	public interface IAgentManager
	{
		Task Begin(string accountId, string instagramAccountId);
	}
}