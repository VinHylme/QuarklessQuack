using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Quarkless.EmailServices.Models
{
	public class UsedBy
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		public string _id { get; set; }
		public bool HasFailed { get; set; }
		public bool Virgin { get; set; }
		public int By { get; set; }
	}
}