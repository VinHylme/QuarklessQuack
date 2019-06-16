using MongoDB.Bson.Serialization.Attributes;
using System;

namespace QuarklessContexts.Models.Logger
{
	public class LoggerModel
	{
		[BsonId]
		[BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
		public string _id {get; set;}
		public string Message { get; set; }
		public DateTime Date { get; set; }
		public string AccountId { get; set; }
		public string InstagramUsername { get; set; }
		public string Type { get; set; }
	}
}
