using System.Threading.Tasks;

namespace Quarkless.Run.Services.Automation.Models.Interfaces
{
	public interface IAgentManager
	{
		Task Start(string accountId, string instagramAccountId);
	}
}