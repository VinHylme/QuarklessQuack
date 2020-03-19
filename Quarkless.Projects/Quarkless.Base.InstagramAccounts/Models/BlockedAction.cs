using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Quarkless.Models.Common.Enums;

namespace Quarkless.Base.InstagramAccounts.Models
{
	public class BlockedAction
	{
		[BsonRepresentation(BsonType.Int32)]
		public ActionType ActionType { get; set; }

		[BsonRepresentation(BsonType.DateTime)]
		public DateTime DateBlocked { get; set; }
	}
}