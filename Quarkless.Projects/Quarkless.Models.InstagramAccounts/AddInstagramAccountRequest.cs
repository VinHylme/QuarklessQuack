namespace Quarkless.Models.InstagramAccounts
{
	public class AddInstagramAccountRequest
	{
		public string Username { get; set; }
		public string Password { get; set; }
		public string ComingFrom { get; set; }
		public int Type { get; set; }
	}
}
