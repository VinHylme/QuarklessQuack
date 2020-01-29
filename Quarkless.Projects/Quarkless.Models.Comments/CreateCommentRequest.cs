using Quarkless.Models.Common.Interfaces;

namespace Quarkless.Models.Comments
{
	public class CreateCommentRequest : IExec
	{
		public string MediaId { get; set; }
		public string Text { get; set; }
	}
}