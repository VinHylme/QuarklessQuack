using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace Quarkless.Models.Library
{
	public class HashtagsLib
	{
		[BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
		[BsonId]
		public string _id { get; set; }
		[BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
		public string InstagramAccountId { get; set; }
		public string AccountId { get; set;  }
		public string GroupName { get; set;  }
		public List<string> Hashtag { get; set; }
		public DateTime DateAdded { get; set;  }
	}
}