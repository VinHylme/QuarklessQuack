using Quarkless.Models.Common.Models;

namespace Quarkless.Models.Profile
{
	public class ProfilePublishEventModel
	{
		public ProfileModel Profile { get; set; }
		public string IpAddress { get; set; }
		public Location Location { get; set; }
	}
}