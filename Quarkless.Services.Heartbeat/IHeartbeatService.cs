using System.Threading.Tasks;

namespace Quarkless.Services.Heartbeat
{
	public interface IHeartbeatService
	{
		Task Start();
	}
}