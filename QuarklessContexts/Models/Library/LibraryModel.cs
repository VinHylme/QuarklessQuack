using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;

namespace QuarklessContexts.Models.Library
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
	public class MessagesLib
	{
		[BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
		[BsonId]
		public string _id { get; set; }
		[BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
		public string InstagramAccountId { get; set; }
		public string AccountId { get; set;  }
		public string GroupName { get; set; }
		public string Message { get; set; }
		public DateTime DateAdded { get; set;  }
	}
	public enum MediaSelectionType
	{
		[Description("image")]
		Image = 0,
		[Description("video")]
		Video = 1,
		[Description("carousel")]
		Carousel = 2
	}

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
