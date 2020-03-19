using System;
using MongoDB.Bson.Serialization.Attributes;

namespace Quarkless.Base.InstagramComments.Models
{
	public class CommentCorpus
	{
		[BsonId]
		[BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
		public string _id { get; set; }
		public string MediaId { get; set; }
		public string Comment { get; set; }
		public string Language { get; set; }
		public bool IsReply { get; set; }
		public DateTime Created { get; set; }
		public int NumberOfLikes { get; set; }
		public int NumberOfReplies { get; set; }
		public string Username { get; set; }
		public From From { get; set; }
	}
}
