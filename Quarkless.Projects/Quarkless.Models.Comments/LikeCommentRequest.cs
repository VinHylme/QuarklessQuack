using Quarkless.Models.Common.Interfaces;

namespace Quarkless.Models.Comments
{
	public class LikeCommentRequest : IExec
	{
		public long CommentId { get; set; }
	}
}