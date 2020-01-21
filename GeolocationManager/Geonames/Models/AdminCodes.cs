using Newtonsoft.Json;

namespace GeolocationManager.Geonames.Models
{
	public class AdminCodes
	{
		[JsonProperty("ISO3166_2")]
		public string Lang { get; set; }
	}
}