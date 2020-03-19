using System.Threading.Tasks;
using ExtractOperationType = Quarkless.Run.Services.Heartbeat.Models.Enums.ExtractOperationType;

namespace Quarkless.Run.Services.Heartbeat.Models.Interfaces
{
	public interface IHeartbeatService
	{
		Task Start(CustomerAccount customer, ExtractOperationType operation);
	}
}