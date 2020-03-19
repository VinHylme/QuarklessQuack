using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Quarkless.Base.Library.Models
{
	public class CaptionsLib
	{
		[BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
		[BsonId]
		public string _id { get; set; }
		[BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
		public string InstagramAccountId { get; set; }
		public string AccountId { get; set;  }
		public string GroupName { get; set; }
		public string Caption { get; set; }
		public DateTime DateAdded { get; set;  }
	}
}