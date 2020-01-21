using InstagramApiSharp.Classes.Models;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Heartbeat.Interfaces;
using Quarkless.Models.SearchResponse;
using System.Collections.Generic;

namespace Quarkless.Models.Heartbeat
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
