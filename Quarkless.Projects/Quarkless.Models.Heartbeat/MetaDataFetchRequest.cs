using Quarkless.Models.Common.Enums;
using Quarkless.Models.Heartbeat.Interfaces;

namespace Quarkless.Models.Heartbeat
{
	public class MetaDataFetchRequest : IMetaDataRequest
	{
		public MetaDataType MetaDataType { get; set; }
		public string ProfileCategoryTopicId { get; set; }
		public string InstagramId { get; set; }
		public string AccountId { get; set; }
	}
}