using System.Threading.Tasks;
using QuarklessContexts.Enums;

namespace Quarkless.Services.Heartbeat
{
	public struct CustomerAccount
	{
		public string UserId { get; set; }
		public string InstagramAccountId { get; set; }
	}
	public interface IHeartbeatService
	{
		Task Start(CustomerAccount customer, ExtractOperationType operation);
	}
}