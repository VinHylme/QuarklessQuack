using System;

namespace QuarklessContexts.Models.ServicesModels.SearchModels
{
	public class CommentResponse
	{
		public long CommentId { get; set; }
		public long LikeCount { get; set; }
		public int TotalChildComments { get; set; }
		public bool DidReportForSpam { get; set; }
		public string Text { get; set; }
		public bool HaslikedBefore { get; set; }
		public DateTime CreatedAt { get; set; }
		public string Status { get; set; }
	}
}
