using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuarklessContexts.Models.ServicesModels.Corpus
{
	public class CommentCorpus
	{
		[BsonId]
		[BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)] 
		public string _id { get; set; }
		public string Topic { get; set; }
		public string MediaId { get ;set; }
		public string Comment { get; set; }
		public string Language { get; set; }
		public bool IsReply { get; set; }
		public DateTime Created { get; set; }
		public int NumberOfLikes { get; set; }
		public int NumberOfReplies { get; set; }
		public string Username { get; set; }
	}
}
