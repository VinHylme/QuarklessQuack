using Newtonsoft.Json;

namespace Quarkless.Models.Auth
{
	public class LocationDetails
	{
		[JsonProperty("city")]
		public string City { get; set; }

		[JsonProperty("continent")]
		public string Continent { get; set; }

		[JsonProperty("country")]
		public string Country { get; set; }

		[JsonProperty("countryCode")]
		public string CountryCode { get; set; }

		[JsonProperty("region")]
		public string Region { get; set; }

		[JsonProperty("regionOtherNames")]
		public string RegionOtherNames { get; set; }
		
		[JsonProperty("location")]
		public LocationPosition Position { get; set; }
	}
}