using Newtonsoft.Json;

namespace GeolocationManager.GoogleGeocode.Models
{
	public class PlusCode
	{
		[JsonProperty("compound_code")]
		public string CompoundCode { get; set; }

		[JsonProperty("global_code")]
		public string GlobalCode { get; set; }
	}
}