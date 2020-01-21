namespace Quarkless.Models.Query
{
	public class ProfileRequest
	{
		public string AccountId { get; set; }
		public string InstagramAccountId { get; set; }
		public Profile.Topic Topic { get; set; }
	}
}