using Newtonsoft.Json;

namespace GeolocationManager.GoogleGeocode.Models
{
	public class Bounds
	{
		[JsonProperty("northeast")]
		public Northeast Northeast { get; set; }

		[JsonProperty("southwest")]
		public Southwest Southwest { get; set; }
	}
}