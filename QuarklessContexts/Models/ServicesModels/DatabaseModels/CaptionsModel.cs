using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuarklessContexts.Models.ServicesModels.DatabaseModels
{
	public class CaptionsModel
	{
		[BsonId]
		[BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
		public string _id { get; set; }
		public string MediaId { get; set; }
		public DateTime DateCreated { get; set; }
		public string Text { get; set; }
		public string Topic { get; set; }
		public string User_Username { get; set; }
		public long User_Id { get; set; }
		[BsonIgnore]
		public string Language { get; set; }
		public int NumberOfLikesOnMedia { get; set; }
	}
}
