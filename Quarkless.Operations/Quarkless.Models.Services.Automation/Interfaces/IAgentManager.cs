using System.Threading.Tasks;

namespace Quarkless.Models.Services.Automation.Interfaces
{
	public interface IAgentManager
	{
		Task Begin(string accountId, string instagramAccountId);
		Task Start(string accountId, string instagramAccountId);
	}
}