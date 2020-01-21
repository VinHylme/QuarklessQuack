using Newtonsoft.Json;

namespace GeolocationManager.GoogleGeocode.Models
{
	public class Location
	{
		[JsonProperty("lat")]
		public float Latitude { get; set; }

		[JsonProperty("lng")]
		public float Longitude { get; set; }
	}
}