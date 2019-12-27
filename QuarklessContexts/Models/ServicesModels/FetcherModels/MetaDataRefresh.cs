using System.Collections.Generic;
using InstagramApiSharp.Classes.Models;
using QuarklessContexts.Models.ServicesModels.HeartbeatModels;
using QuarklessContexts.Models.ServicesModels.SearchModels;

namespace QuarklessContexts.Models.ServicesModels.FetcherModels
{
	public class MetaDataMediaRefresh
	{
		public MetaDataType MetaDataType { get; set; }
		public string Topic { get; set; }
		public string UserId { get; set; }
		public List<MediaResponse> Medias;
	}
	public class MetaDataCommentRefresh
	{
		public MetaDataType MetaDataType { get; set; }
		public string Topic { get; set; }
		public string UserId { get; set; }
		public List<UserResponse<InstaComment>> Comments;
	}
}
