using System.Collections.Generic;
using InstagramApiSharp.Classes.Models;
using QuarklessContexts.Models.ServicesModels.HeartbeatModels;
using QuarklessContexts.Models.ServicesModels.SearchModels;

namespace QuarklessContexts.Models.ServicesModels.FetcherModels
{
	public class MetaDataMediaRefresh : IMetaDataRequest
	{
		public string Uuid { get; set; }
		public MetaDataType MetaDataType { get; set; }
		public string ProfileCategoryTopicId { get; set; }
		public string InstagramId { get; set; }
		public List<MediaResponse> Medias;
	}
	public class MetaDataCommentRefresh : IMetaDataRequest
	{
		public string Uuid { get; set; }
		public MetaDataType MetaDataType { get; set; }
		public string ProfileCategoryTopicId { get; set; }
		public string InstagramId { get; set; }
		public List<UserResponse<InstaComment>> Comments;
	}
}
