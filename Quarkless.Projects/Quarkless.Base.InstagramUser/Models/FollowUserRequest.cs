using Quarkless.Models.Common.Interfaces;
using Quarkless.Models.Common.Models.Resolver;

namespace Quarkless.Base.InstagramUser.Models
{
	public class FollowAndUnFollowUserRequest : IExec
	{
		public long UserId { get; set; }
		public UserShort User { get; set; }
		public DataFrom DataFrom { get; set; }
	}
}