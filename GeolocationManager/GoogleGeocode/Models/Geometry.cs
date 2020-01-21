using Newtonsoft.Json;

namespace GeolocationManager.GoogleGeocode.Models
{
	public class Geometry
	{
		[JsonProperty("location")]
		public Location Location { get; set; }

		[JsonProperty("location_type")]
		public string LocationType { get; set; }

		[JsonProperty("viewport")]
		public Viewport Viewport { get; set; }

		[JsonProperty("bounds")]
		public Bounds Bounds { get; set; }
	}
}