using Quarkless.Models.Common.Interfaces;
using Quarkless.Models.Common.Models.Resolver;

namespace Quarkless.Base.InstagramComments.Models
{
	public class LikeCommentRequest : IExec
	{
		public long CommentId { get; set; }
		public string CommentLiked { get; set; }
		public UserShort User { get; set; }
		public DataFrom DataFrom { get; set; }
	}
}