using System;
using MongoDB.Bson.Serialization.Attributes;

namespace Quarkless.Models.Library
{
	public class MessagesLib
	{
		[BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
		[BsonId]
		public string _id { get; set; }
		[BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
		public string InstagramAccountId { get; set; }
		public string AccountId { get; set;  }
		public string GroupName { get; set; }
		public MessageEntity Entity { get; set; }
		public int Type { get; set; }
		public DateTime DateAdded { get; set;  }
	}
}