using Newtonsoft.Json;

namespace GeolocationManager.GoogleGeocode.Models
{
	public class Result
	{
		[JsonProperty("address_components")]
		public AddressComponents[] AddressComponents { get; set; }

		[JsonProperty("formatted_address")]
		public string FormattedAddress { get; set; }

		[JsonProperty("geometry")]
		public Geometry Geometry { get; set; }

		[JsonProperty("place_id")]
		public string PlaceId { get; set; }

		[JsonProperty("plus_code")]
		public PlusCode PlusCode { get; set; }

		[JsonProperty("types")]
		public string[] Types { get; set; }
	}
}