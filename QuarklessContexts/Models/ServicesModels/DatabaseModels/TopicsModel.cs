using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace QuarklessContexts.Models.ServicesModels.DatabaseModels
{
	public class SubTopics
	{
		public string Topic { get; set; }
		public List<string> RelatedTopics { get; set; }
	}
	public class TopicsModel
	{
		[BsonId]
		[BsonIgnoreIfDefault]
		[BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
		public string _id { get; set; }
		public string TopicName { get; set; }
		public List<SubTopics> SubTopics { get; set; }
	}

}
