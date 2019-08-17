using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;

namespace QuarklessContexts.Models.Library
{
	public class HashtagsLib
	{
		public string InstagramAccountId { get; set; }
		public string GroupName { get; set;  }
		public List<string> Hashtag;
		public DateTime DateAdded { get; set;  }
	}
	public class CaptionsLib
	{
		public string InstagramAccountId { get; set; }
		public string GroupName { get; set; }
		public string Caption;
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
		public string InstagramAccountId { get; set; }
		public string GroupName { get; set; }
		public MediaSelectionType MediaType { get; set;  }
		public DateTime DateAdded { get; set;  }
		public string MediaBytes { get; set;  }
		public string MediaUrl { get; set;  }
	}
	public class LibraryModel
	{
		[BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
		[BsonId]
		public string _id { get; set; }
		public string AccountId { get; set;  }
		[BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
		public string InstagramAccountId {get; set;}
		public HashtagsLib SavedHashtags { get; set; }
		public CaptionsLib SavedCaptions { get; set; }
		public MediasLib SavedMedias { get; set; }
	}
}
