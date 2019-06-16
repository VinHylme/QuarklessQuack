using InstagramApiSharp.Classes.Models;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuarklessContexts.Models.DataAnalysers
{
	public class CommentsAnalyserModel
	{
		[BsonId]
		[BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
		public string _id {get; set;}
		public string SearchQuery { get; set; }
		public DateTime QueryTime { get; set; }
		public InstaCommentList CommentList { get; set; }
	}
}
