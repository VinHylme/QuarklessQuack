using QuarklessContexts.Models.Profiles;

namespace QuarklessContexts.Models.ServicesModels.SearchModels
{
	public class ProfileRequest
	{
		public string AccountId { get; set; }
		public string InstagramAccountId { get; set; }
		public Topic Topic { get; set; }
	}
}