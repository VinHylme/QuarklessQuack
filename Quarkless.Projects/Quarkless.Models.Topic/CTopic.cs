using MongoDB.Bson.Serialization.Attributes;

namespace Quarkless.Models.Topic
{
	public class CTopic
	{
		[BsonId]
		[BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
		public string _id { get; set; }
		[BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
		public string ParentTopicId { get; set; }
		public string Name { get; set; }
	}
}