using System;
using System.Collections.Generic;
using System.Linq;
using InstagramApiSharp.Classes.Models;
using Quarkless.Models.Common.Extensions;

namespace Quarkless.Base.Analytics.Models
{
	public class HashtagMedia
	{
		public string ParentHashtag { get; set; }
		public string MediaId { get; set; }
		public string PostUrl { get; set; }
		public long UnixTakenAt { get; set; }
		public DateTime TakenAt { get; set; }
		public InstaMediaType MediaType { get; set; }
		public string FilterType { get; set; }
		public List<InstaImage> Images { get; set; } = new List<InstaImage>();
		public List<InstaVideo> Videos { get; set; } = new List<InstaVideo>();
		public int Width { get; set; }
		public string Height { get; set; }
		public int LikesCount { get; set; }
		public int ViewCount { get; set; }
		public string CommentsCount { get; set; }
		public bool IsCommentingDisabled { get; set; }
		public bool HasLiked { get; set; }
		public bool HasAudio { get; set; }
		public InstaLocation Location { get; set; }
		public List<InstaUserTag> UserTags { get; set; } = new List<InstaUserTag>();
		public InstaUserShortList Likers { get; set; } = new InstaUserShortList();
		public string Caption { get; set; }
		public IEnumerable<RelatedHashtag> Hashtags => Caption?
			.FilterHashtags().Select(_=>new RelatedHashtag
			{
				Id = long.Parse(MediaId),
				Type = "media",
				TagName = _,
				SimilarityDistanceToParent = _.RemoveFirstHashtagCharacter()
					.Similarity(ParentHashtag)
			});

		public InstaUser User { get; set; }
	}
}