using Quarkless.Base.Heartbeat.Models.Interfaces;
using Quarkless.Models.Common.Enums;

namespace Quarkless.Base.Heartbeat.Models
{
	public class MetaDataFetchRequest : IMetaDataRequest
	{
		public MetaDataType MetaDataType { get; set; }
		public string ProfileCategoryTopicId { get; set; }
		public string InstagramId { get; set; }
		public string AccountId { get; set; }
	}
}