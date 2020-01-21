using Newtonsoft.Json;

namespace GeolocationManager.Geonames.Models
{
	public class GeoName
	{
		[JsonProperty("adminCode1")]
		public string AdminCode { get; set; }

		[JsonProperty("lng")]
		public string Longitude { get; set; }

		[JsonProperty("distance")]
		public string Distance { get; set; }

		[JsonProperty("geonameId")]
		public int GeonameId { get; set; }

		[JsonProperty("toponymName")]
		public string ToponymName { get; set; }

		[JsonProperty("countryId")]
		public string CountryId { get; set; }

		[JsonProperty("fcl")]
		public string Fcl { get; set; }

		[JsonProperty("population")]
		public int Population { get; set; }

		[JsonProperty("countryCode")]
		public string CountryCode { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("fclName")]
		public string FclName { get; set; }

		[JsonProperty("adminCodes1")]
		public AdminCodes AdminCodes { get; set; }

		[JsonProperty("countryName")]
		public string CountryName { get; set; }

		[JsonProperty("fcodeName")]
		public string FCodeName { get; set; }

		[JsonProperty("adminName1")]
		public string AdminName { get; set; }

		[JsonProperty("lat")]
		public string Latitude { get; set; }

		[JsonProperty("fcode")]
		public string FCode { get; set; }
	}
}