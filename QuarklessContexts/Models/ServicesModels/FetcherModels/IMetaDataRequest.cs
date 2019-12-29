using QuarklessContexts.Models.ServicesModels.HeartbeatModels;

namespace QuarklessContexts.Models.ServicesModels.FetcherModels
{
	public interface IMetaDataRequest
	{
		MetaDataType MetaDataType { get; set; }
		string ProfileCategoryTopicId { get; set; }
		string InstagramId { get; set; }
		string AccountId { get; set; }
	}

	public class MetaDataFetchRequest : IMetaDataRequest
	{
		public MetaDataType MetaDataType { get; set; }
		public string ProfileCategoryTopicId { get; set; }
		public string InstagramId { get; set; }
		public string AccountId { get; set; }
	}
	public class MetaDataContainsRequest : IMetaDataRequest
	{
		public MetaDataType MetaDataType { get; set; }
		public string ProfileCategoryTopicId { get; set; }
		public string InstagramId { get; set; }
		public string AccountId { get; set; }
		public string JsonData { get; set; }
	}
	public class MetaDataCommitRequest<TInput> : IMetaDataRequest
	{
		public MetaDataType MetaDataType { get; set; }
		public string ProfileCategoryTopicId { get; set; }
		public string InstagramId { get; set; }
		public string AccountId { get; set; }
		public Meta<TInput> Data { get; set; }
	}
}