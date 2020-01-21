using System;
using MongoDB.Bson.Serialization.Attributes;
using Quarkless.Models.Common.Enums;

namespace Quarkless.Models.Library
{
	public class MediasLib
	{
		[BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
		[BsonId]
		public string _id { get; set; }
		[BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
		public string InstagramAccountId { get; set; }
		public string AccountId { get; set;  }
		public string GroupName { get; set; }
		public MediaSelectionType MediaType { get; set;  }
		public DateTime DateAdded { get; set;  }
		public string MediaBytes { get; set;  }
		public string MediaUrl { get; set;  }
	}
}