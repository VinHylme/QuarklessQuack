using System.Threading.Tasks;

namespace Quarkless.Models.Services.Automation.Interfaces
{
	public interface IAgentManager
	{
		Task Start(string accountId, string instagramAccountId);
	}
}