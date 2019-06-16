using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuarklessContexts.Models.ServicesModels.DatabaseModels
{
	
	public class UserBiographyModel
	{
		[BsonId]
		[BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
		public string _id { get; set; }
		public string Topic { get; set; }
		public User User { get; set; }
		public Contact Contact { get; set; }
		public string Text { get; set; }
		public string Language { get; set; }
	}
}
