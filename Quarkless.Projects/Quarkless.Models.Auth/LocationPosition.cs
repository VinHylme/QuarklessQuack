using Newtonsoft.Json;

namespace Quarkless.Models.Auth
{
	public class LocationPosition
	{
		[JsonProperty("latitude")]
		public double Latitude { get; set; }
		[JsonProperty("longitude")]
		public double Longitude { get; set; }
	}
}