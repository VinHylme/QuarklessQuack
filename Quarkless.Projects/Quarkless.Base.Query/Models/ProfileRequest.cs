
namespace Quarkless.Base.Query.Models
{
	public class ProfileRequest
	{
		public string AccountId { get; set; }
		public string InstagramAccountId { get; set; }
		public Quarkless.Models.Common.Models.Topic.Topic Topic { get; set; }
	}
}