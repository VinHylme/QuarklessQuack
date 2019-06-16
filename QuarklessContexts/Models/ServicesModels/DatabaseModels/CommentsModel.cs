using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuarklessContexts.Models.ServicesModels.DatabaseModels
{
	public class Reply
	{
		public long InstaCommentId { get; set; }
		public string Text { get; set; }
		public int Comment_LikesCount { get; set; }
		public DateTime CommentMadeAt { get; set; }
		public string User_Username { get; set; }
		public long User_Id { get; set; }

	}
	public class CommentsModel
	{
		[BsonId]
		[BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
		public string _id { get; set; }
		public List<Reply> PreviewReplies { get; set; }
		public string Text { get; set; }
		public int Media_CommentCount { get; set; }
		public int Media_LikesCount { get; set; }
		public int Media_ViewsCount { get; set; }
		public int Comment_LikesCount { get; set; }
		public int Comment_Replies { get; set; }
		public DateTime CommentMadeAt { get; set; }
		public string User_Username { get; set; }
		public long User_Id { get; set; }
		public long InstaCommentId {get; set;}
		public string MediaId { get; set; }
		public string Topic { get; set; }
		[BsonIgnore]
		public string Language { get; set; }

		public CommentsModel()
		{
			PreviewReplies = new List<Reply>();
		}
	}
}
