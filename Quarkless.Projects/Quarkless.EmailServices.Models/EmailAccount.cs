using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Quarkless.Utilities.Models.Person;

namespace Quarkless.EmailServices.Models
{
	public class EmailAccount
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		public string _id { get; set; }
		public string Topic { get; set; }
		public PersonModel Person { get; set; }
		public List<UsedBy> UsedBy { get; set; } = new List<UsedBy>();
		public DateTime CreationDate { get; set; }
	}
}