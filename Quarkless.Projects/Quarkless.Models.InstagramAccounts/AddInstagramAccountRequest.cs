using Newtonsoft.Json;

namespace Quarkless.Models.InstagramAccounts
{
	public class AddInstagramAccountRequest
	{
		[JsonProperty("Username")]
		public string Username { get; set; }

		[JsonProperty("Password")]
		public string Password { get; set; }

		[JsonProperty("IpAddress")]
		public string IpAddress { get; set; }

		[JsonProperty("Location")]
		public string Location { get; set; }

		[JsonProperty("Type")]
		public int Type { get; set; }
	}
}
