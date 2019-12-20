using System.Collections.Generic;
using InstagramApiSharp.Classes.Models;

namespace QuarklessContexts.Models.ServicesModels.SearchModels
{
	public class CommentMedia
	{
		public MediaResponseSingle Media { get; set; }
		public List<UserResponse<InstaComment>> Comments { get; set; }
	}
}