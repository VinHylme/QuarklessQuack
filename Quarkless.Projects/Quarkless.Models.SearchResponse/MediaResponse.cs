using System;
using System.Collections.Generic;
using InstagramApiSharp.Classes.Models;
using Quarkless.Models.SearchResponse.Enums;
using Quarkless.Models.Topic;

namespace Quarkless.Models.SearchResponse
{
	[Serializable]
	public class MediaResponse
	{
		public DateTime TakenAt { get; set; }
		public CTopic Topic { get; set; }
		public string Domain { get; set; }
		public string Title { get; set; }
		public List<string> MediaUrl { get; set; } = new List<string>();
		public int LikesCount { get; set; }
		public string Caption { get; set; }
		public InstaMediaExplore Explore { get; set; }
		public int ViewCount { get; set; }
		public string CommentCount { get; set; }
		public string MediaId { get; set; }
		public bool HasLikedBefore { get; set; }
		public bool HasAudio { get; set; }
		public int NumberOfQualities { get; set; }
		public bool PhotosOfI { get; set; }
		public List<InstaComment> PreviewComments { get; set; }
		public List<InstaProductTag> ProductTags { get; set; }
		public List<InstaUserTag> UserTags { get; set; }
		public InstaMediaIdList TopLikers { get; set; }
		public string ProductType { get; set; }
		public bool HasSeen { get; set; }
		public string FilterType { get; set; }
		public bool? IsFollowing { get; set; }
		public bool IsCommentsDisabled { get; set; }
		public InstaLocation Location { get; set; }
		public InstaMediaType MediaType { get; set; }
		public MediaFrom MediaFrom { get; set; }
		public UserResponse User { get; set; }
	}
}