using System.Threading.Tasks;
using Quarkless.Models.Services.Heartbeat.Enums;

namespace Quarkless.Models.Services.Heartbeat.Interfaces
{
	public interface IHeartbeatService
	{
		Task Start(CustomerAccount customer, ExtractOperationType operation);
	}
}