using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuarklessContexts.Models.ServicesModels.DatabaseModels
{
	public class TopicsModel
	{
		[BsonId]
		[BsonIgnoreIfDefault]
		[BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
		public string _id { get; set; }
		public string TopicName { get; set; }
		public List<string> SubTopics { get; set; }
	}
}
