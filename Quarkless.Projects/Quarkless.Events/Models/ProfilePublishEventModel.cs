using Quarkless.Events.Models.PublishObjects;
using Location = Quarkless.Models.Common.Models.Location;

namespace Quarkless.Events.Models
{
	public class ProfilePublishEventModel
	{
		public ProfileModel Profile { get; set; }
		public string IpAddress { get; set; }
		public Location Location { get; set; }
		public ProxyModelShort UserProxy { get; set; }
	}
}