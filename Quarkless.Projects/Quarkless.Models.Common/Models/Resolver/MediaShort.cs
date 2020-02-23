using System.Collections.Generic;

namespace Quarkless.Models.Common.Models.Resolver
{
	public class MediaShort
	{
		public string Id { get; set; }
		public string MediaUrl { get; set; }
		public int LikesCount { get; set; }
		public int CommentCount { get; set; }
		public bool IncludedInMedia { get; set; }
		public List<string> Hashtags { get; set; }
		public string Caption { get; set; }
		public int MediaType { get; set; }
	}
}