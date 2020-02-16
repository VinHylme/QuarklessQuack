using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Timeline.Enums;

namespace Quarkless.Models.Timeline
{
	public class TimelineEventLog
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		public string _id { get; set; }

		[BsonRepresentation(BsonType.ObjectId)]
		public string InstagramAccountId { get; set; }
		public string AccountId { get; set; }

		[BsonRepresentation(BsonType.Int32)]
		public TimelineEventStatus Status { get; set; }

		[BsonRepresentation(BsonType.Int32)]
		public ActionType ActionType { get; set; }

		public string Message { get; set; }
		public Exception Exception { get; set; }
		public string Request { get; set; }
		public string Response { get; set; }
		public int Level { get; set; }

		[BsonRepresentation(BsonType.DateTime)]
		public DateTime DateAdded { get; set; } = DateTime.UtcNow;

		[BsonRepresentation(BsonType.DateTime)]
		[BsonElement("expiresAt")]
		public DateTime ExpiresAt { get; set; }
	}
}