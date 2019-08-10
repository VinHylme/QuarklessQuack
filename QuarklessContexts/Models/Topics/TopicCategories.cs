using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuarklessContexts.Models.Topics
{
	public class TopicCategories
	{
		[BsonId]
		[BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
		private string _id { get; set; }
		public string CategoryName { get; set; }
		public List<string> SubCategories { get; set ;}
	}
}
