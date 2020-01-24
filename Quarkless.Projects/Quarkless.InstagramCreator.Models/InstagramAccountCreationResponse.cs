using Quarkless.InstagramCreator.Models.Enums;

namespace Quarkless.InstagramCreator.Models
{
	public class InstagramAccountCreationResponse
	{
		public InstagramAccountCreationEnum State { get; set; }
		public InstagramAccount Account { get; set; }
	}
}
