using Newtonsoft.Json;

namespace GeolocationManager.GoogleGeocode.Models
{
	public class Southwest
	{
		[JsonProperty("lat")]
		public float Latitude { get; set; }

		[JsonProperty("lng")]
		public float Longitude { get; set; }
	}
}