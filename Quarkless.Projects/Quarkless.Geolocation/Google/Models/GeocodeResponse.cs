using Newtonsoft.Json;

namespace Quarkless.Geolocation.Google.Models
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

	public class PlusCode
	{
		[JsonProperty("compound_code")]
		public string CompoundCode { get; set; }

		[JsonProperty("global_code")]
		public string GlobalCode { get; set; }
	}
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
	public class Location
	{
		[JsonProperty("lat")]
		public float Latitude { get; set; }

		[JsonProperty("lng")]
		public float Longitude { get; set; }
	}
	public class Viewport
	{
		[JsonProperty("northeast")]
		public Northeast Northeast { get; set; }

		[JsonProperty("southwest")]
		public Southwest Southwest { get; set; }
	}
	public class Northeast
	{
		[JsonProperty("lat")]
		public float Latitude { get; set; }

		[JsonProperty("lng")]
		public float Longitude { get; set; }
	}
	public class Southwest
	{
		[JsonProperty("lat")]
		public float Latitude { get; set; }

		[JsonProperty("lng")]
		public float Longitude { get; set; }
	}
	public class Bounds
	{
		[JsonProperty("northeast")]
		public Northeast Northeast { get; set; }

		[JsonProperty("southwest")]
		public Southwest Southwest { get; set; }
	}
	public class AddressComponents
	{
		[JsonProperty("long_name")]
		public string LongName { get; set; }

		[JsonProperty("short_name")]
		public string ShortName { get; set; }

		[JsonProperty("types")]
		public string[] Types { get; set; }
	}

}
