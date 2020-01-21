using Newtonsoft.Json;

namespace GeolocationManager.GoogleGeocode.Models
{
	public class Northeast
	{
		[JsonProperty("lat")]
		public float Latitude { get; set; }

		[JsonProperty("lng")]
		public float Longitude { get; set; }
	}
}