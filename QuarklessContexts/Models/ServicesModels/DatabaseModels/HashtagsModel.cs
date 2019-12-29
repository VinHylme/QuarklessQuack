using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using QuarklessContexts.Models.ServicesModels.Corpus;

namespace QuarklessContexts.Models.ServicesModels.DatabaseModels
{
	public class HashtagsModel
	{
		[BsonId]
		[BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
		public string _id { get; set; }
		public List<string> Hashtags { get; set; }
		public string Language { get; set; }
		public From From { get; set; }
		public HashtagsModel()
		{
			Hashtags = new List<string>();
		}
	}
}
