using InstagramApiSharp.Classes.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuarklessContexts.Models.ServicesModels.SearchModels
{
	public class MediaDetail
	{
		public string Topic { get; set; }
		public List<string> MediaUrl { get; set; } = new List<string>();
		public int LikesCount { get; set; }
		public int ViewCount { get; set; }
		public string CommentCount { get; set; }
		public string MediaId { get; set; }
		public bool HasLikedBefore { get; set; }
		public bool HasAudio { get; set; }
		public bool? IsFollowing { get; set; }
		public bool IsCommentsDisabled { get; set; }
		public InstaLocation Location { get ;set; }
	}
}
