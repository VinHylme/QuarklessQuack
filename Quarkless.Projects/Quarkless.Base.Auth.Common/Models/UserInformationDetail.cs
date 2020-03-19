using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace Quarkless.Base.Auth.Common.Models
{
	public class UserInformationDetail
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		public string _id { get; set; }

		[JsonProperty("ipDetails")]
		public IpDetails IpDetails { get; set; }

		[JsonProperty("geoLocationDetails")]
		public LocationDetails GeoLocationDetails { get; set; }

		[JsonProperty("deviceDetails")]
		public DeviceDetails DeviceDetails { get; set; }
	}
}
