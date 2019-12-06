using QuarklessContexts.Models.Profiles;

namespace Quarkless.Services.Heartbeat
{
	public class Assignment
	{
		public FullUserDetail Customer { get; set; }
		public Worker Worker { get; set; }
		public Topics CustomerTopic { get; set; }
	}
}