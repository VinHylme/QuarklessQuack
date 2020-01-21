using Newtonsoft.Json;

namespace GeolocationManager.IpInfo.Models
{
	public class IpGeolocation
	{
		[JsonProperty("ip")]
		public string Ip { get; set; }

		[JsonProperty("hostname")]
		public string Hostname { get; set; }

		[JsonProperty("city")]
		public string City { get; set; }

		[JsonProperty("region")]
		public string Region { get; set; }

		[JsonProperty("country")]
		public string Country { get; set; }

		[JsonProperty("loc")]
		public string Location { get; set; } //lat,lon

		[JsonProperty("org")]
		public string Organisation { get; set; }

		[JsonProperty("postal")]
		public string PostalCode { get; set; }

		[JsonProperty("timezone")]
		public string Timezone { get; set; }
	}

}
