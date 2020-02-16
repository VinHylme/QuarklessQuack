using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Common.Models.Carriers;
using Quarkless.Models.Timeline.Enums;

namespace Quarkless.Models.Timeline
{
	public class ActionExecuteLog
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		public string _id { get; set; }

		[BsonRepresentation(BsonType.ObjectId)]
		public string InstagramAccountId { get; set; }
		public string InstagramAccountUsername { get; set; }
		public string AccountId { get; set; }

		[BsonRepresentation(BsonType.Int32)]
		public ActionExecuteStatus Status { get; set; }

		[BsonRepresentation(BsonType.Int32)]
		public ActionType ActionType { get; set; }
		public ErrorResponse Error { get; set; }

		[BsonRepresentation(BsonType.DateTime)]
		public DateTime DateAdded { get; set; } = DateTime.UtcNow;
	}
}