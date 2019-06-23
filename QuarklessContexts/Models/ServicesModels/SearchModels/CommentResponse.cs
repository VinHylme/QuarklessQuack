using System;
using System.Collections.Generic;
using System.Text;

namespace QuarklessContexts.Models.ServicesModels.SearchModels
{
	public class CommentResponse
	{
		public long CommentId { get; set; }
		public long LikeCount { get; set; }
		public int TotalChildComments { get; set; }
		public bool DidReportForSpam { get; set; }
		public string Text { get; set; }
	}
}
