using System.Collections.Generic;
using InstagramApiSharp.Classes.Models;
using Quarkless.Base.Heartbeat.Models.Interfaces;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.SearchResponse;

namespace Quarkless.Base.Heartbeat.Models
{
	public class MetaDataCommentRefresh : IMetaDataRequest
	{
		public string Uuid { get; set; }
		public MetaDataType MetaDataType { get; set; }
		public string ProfileCategoryTopicId { get; set; }
		public string InstagramId { get; set; }
		public string AccountId { get; set; }
		public List<UserResponse<InstaComment>> Comments;
	}
}
