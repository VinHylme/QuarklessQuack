using Newtonsoft.Json;

namespace GeolocationManager.GoogleGeocode.Models
{
	public class AddressComponents
	{
		[JsonProperty("long_name")]
		public string LongName { get; set; }

		[JsonProperty("short_name")]
		public string ShortName { get; set; }

		[JsonProperty("types")]
		public string[] Types { get; set; }
	}
}