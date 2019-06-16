using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace QuarklessContexts.Models.Proxies
{
	public class AssignedTo
	{
		public string Account_Id { get; set; }
		[BsonRepresentation(BsonType.ObjectId)]
		public string InstaId { get; set; }
	}
	public class ProxyModel
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		public ObjectId _id { get; set; }
		public AssignedTo AssignedTo { get; set; }
		public string Address { get; set; }
		public int Port { get; set; }
		public bool NeedServerAuth { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }

	}
}
