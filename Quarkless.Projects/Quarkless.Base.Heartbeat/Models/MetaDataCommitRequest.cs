using Quarkless.Base.Heartbeat.Models.Interfaces;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Common.Models;

namespace Quarkless.Base.Heartbeat.Models
{
	public class MetaDataCommitRequest<TInput> : IMetaDataRequest
	{
		public MetaDataType MetaDataType { get; set; }
		public string ProfileCategoryTopicId { get; set; }
		public string InstagramId { get; set; }
		public string AccountId { get; set; }
		public Meta<TInput> Data { get; set; }
	}
}