using Newtonsoft.Json;

namespace GeolocationManager.Geonames.Models
{
	public class NearbyResponse
	{
		[JsonProperty("geonames")]
		public GeoName[] Geonames { get; set; }
	}
}
