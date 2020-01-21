using Newtonsoft.Json;

namespace GeolocationManager.GoogleGeocode.Models
{
	public class GeocodeResponse
	{
		[JsonProperty("plus_code")]
		public PlusCode PlusCode { get; set; }

		[JsonProperty("results")]
		public Result[] Results { get; set; }

		[JsonProperty("status")]
		public string Status { get; set; }
	}
}
