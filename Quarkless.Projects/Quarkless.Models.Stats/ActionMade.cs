using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Quarkless.Models.Common.Enums;

namespace Quarkless.Models.Stats
{
	public class ActionMade
	{
		public ActionType ActionType { get; set; }

		[BsonRepresentation(BsonType.DateTime)]
		public DateTime DateActionMade { get; set; }
		public bool WasSuccessful { get; set; }
		public string Request { get; set; }
	}
}