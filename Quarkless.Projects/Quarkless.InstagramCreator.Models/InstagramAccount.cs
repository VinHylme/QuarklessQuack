using System;
using Bogus.DataSets;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Quarkless.InstagramCreator.Models
{
	public class InstagramAccount
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		public string _id { get; set; }
		public string Username { get; set; }
		public string Email { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public Name.Gender Gender { get; set; }
		public string Password { get; set; }
		public string PhoneNumber { get; set; }
		public bool Virgin { get; set; }
		public bool NeedsVerifying { get; set; } = false;
		public string Topic { get; set; }
		public DateTime CreationTime { get; set; }
	}
}