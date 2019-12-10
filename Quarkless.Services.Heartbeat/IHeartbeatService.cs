using System.Threading.Tasks;
using QuarklessContexts.Enums;

namespace Quarkless.Services.Heartbeat
{
	public struct CustomerAccount
	{
		public string UserId { get; set; }
		public string InstagramAccountId { get; set; }
	}
	public struct WorkerAccount
	{
		public string UserId { get; set; }
		public string InstagramAccountId { get; set; }
	}

	public interface IHeartbeatService
	{
		Task Start(CustomerAccount customer, WorkerAccount worker, ExtractOperationType operation);
	}
}