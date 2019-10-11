﻿using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace QuarklessContexts.Models.ServicesModels.DatabaseModels
{
	public class HashtagsModel
	{
		[BsonId]
		[BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
		public string _id { get; set; }
		public List<string> Hashtags { get; set; }
		public string Topic { get; set; }
		public string Language { get; set; }

		public HashtagsModel()
		{
			Hashtags = new List<string>();
		}
	}
}
