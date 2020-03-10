using Newtonsoft.Json;
using Quarkless.Models.Common.Models;

namespace Quarkless.Models.InstagramAccounts
{
	public class ProxyLinkRequest
	{
		public int proxyType { get; set; }
		public string HostAddress { get; set; }
		public string Port { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
	}
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
