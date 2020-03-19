using Newtonsoft.Json;
using Quarkless.Models.Common.Models;

namespace Quarkless.Base.InstagramAccounts.Models
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
		public Location Location { get; set; }

		[JsonProperty("Type")]
		public int Type { get; set; }
		
		public bool EnableAutoLocate { get; set; }

		public bool ShouldGenerateDevice { get; set; } = true;

		[JsonProperty("ProxyDetail")]
		public ProxyLinkRequest ProxyDetail { get; set; }
	}
}
