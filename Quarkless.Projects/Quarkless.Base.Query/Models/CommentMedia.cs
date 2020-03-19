using System.Collections.Generic;
using InstagramApiSharp.Classes.Models;
using Quarkless.Models.SearchResponse;

namespace Quarkless.Base.Query.Models
{
	public class CommentMedia
	{
		public MediaResponseSingle Media { get; set; }
		public List<UserResponse<InstaComment>> Comments { get; set; }
	}
}