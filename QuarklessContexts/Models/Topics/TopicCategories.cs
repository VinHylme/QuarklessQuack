using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace QuarklessContexts.Models.Topics
{

	public class Category
	{
		public string CategoryId { get; set; }
		public string CategoryName { get; set; }
	}

	public class TopicCategory
	{
		[BsonId]
		[BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
		public string _id { get; set; }
		public Category Category { get; set; }
		public List<string> SubCategories { get; set; }
	}
	public class TopicCategories
	{
		[BsonId]
		[BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
		private string _id { get; set; }
		public string CategoryName { get; set; }
		public List<string> SubCategories { get; set ;}
	}
}
