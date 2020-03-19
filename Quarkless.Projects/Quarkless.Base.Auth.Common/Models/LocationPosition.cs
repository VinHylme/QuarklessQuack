using Newtonsoft.Json;

namespace Quarkless.Base.Auth.Common.Models
{
	public class LocationPosition
	{
		[JsonProperty("latitude")]
		public double Latitude { get; set; }
		[JsonProperty("longitude")]
		public double Longitude { get; set; }
	}
}