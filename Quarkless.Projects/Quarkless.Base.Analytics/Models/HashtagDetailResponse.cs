using System.Collections.Generic;

namespace Quarkless.Base.Analytics.Models
{
	public class HashtagDetailResponse
	{
		public HashtagObject Hashtag { get; set; }
		public List<HashtagObject> QueryResults { get; set; }
		public List<RelatedHashtag> RelatedHashtags { get; set; }
		public List<HashtagMedia> TopPosts { get; set; }
		public int TopPostsCount => TopPosts.Count;
		public int RelatedHashtagCount => RelatedHashtags.Count;

		public HashtagDetailResponse()
		{
			QueryResults = new List<HashtagObject>();
			RelatedHashtags = new List<RelatedHashtag>();
			TopPosts = new List<HashtagMedia>();
		}
	}

}
