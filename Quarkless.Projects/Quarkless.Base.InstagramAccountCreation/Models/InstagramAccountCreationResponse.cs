using Quarkless.Base.InstagramAccountCreation.Models.Enums;

namespace Quarkless.Base.InstagramAccountCreation.Models
{
	public class InstagramAccountCreationResponse
	{
		public InstagramAccountCreationEnum State { get; set; }
		public InstagramAccount Account { get; set; }
	}
}
