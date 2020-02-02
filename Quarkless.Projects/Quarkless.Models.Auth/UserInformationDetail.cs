using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace Quarkless.Models.Auth
{
	public class UserInformationDetail
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		public string _id { get; set; }

		[JsonProperty("ipDetails")]
		public IpDetails IpDetail { get; set; }

		[JsonProperty("geoLocationDetails")]
		public LocationDetails LocationDetail { get; set; }

		[JsonProperty("deviceDetails")]
		public DeviceDetails DeviceDetail { get; set; }
	}
}
