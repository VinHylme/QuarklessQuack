using Newtonsoft.Json;

namespace Quarkless.Base.Auth.Common.Models
{
	public class IpDetails
	{
		[JsonProperty("ipAddress")]
		public string IpAddress { get; set; }
		
		[JsonProperty("ipType")]
		public string IpType { get; set; }

		[JsonProperty("ispName")]
		public string IspName { get; set; }

		[JsonProperty("ipName")]
		public string IpName { get; set; }

		[JsonProperty("org")]
		public string Org { get; set; }
	}
}