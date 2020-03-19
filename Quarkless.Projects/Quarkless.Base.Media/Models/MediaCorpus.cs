using System;
using System.Collections.Generic;
using InstagramApiSharp.Classes.Models;
using MongoDB.Bson.Serialization.Attributes;

namespace Quarkless.Base.Media.Models
{
	public class MediaCorpus
	{
		[BsonId]
		[BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
		public string _id { get; set; }
		public string MediaId { get; set; }
		public string Caption { get; set; }
		public string OriginalCaption { get; set; }
		public int ViewsCount { get; set; }
		public int LikesCount { get; set; }
		public string CommentsCount { get; set; }
		public InstaLocation Location { get; set; }
		public List<string> Usertags { get; set; }
		public string Username { get; set; }
		public string Language { get; set; }
		public DateTime TakenAt { get; set; }
		public From From { get; set; }
	}
}