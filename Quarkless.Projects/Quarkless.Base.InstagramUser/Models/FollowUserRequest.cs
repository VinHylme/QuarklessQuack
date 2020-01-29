using Quarkless.Models.Common.Interfaces;

namespace Quarkless.Base.InstagramUser.Models
{
	public class FollowAndUnFollowUserRequest : IExec
	{
		public long UserId { get; set; }
	}
}