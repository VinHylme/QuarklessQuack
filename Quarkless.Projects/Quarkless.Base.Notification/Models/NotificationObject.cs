using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Quarkless.Base.Notification.Models.Enums;

namespace Quarkless.Base.Notification.Models
{
	public class NotificationObject
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		public string _id { get; set; }

		[BsonRepresentation(BsonType.ObjectId)]
		public string InstagramAccountId { get; set; }

		public string AccountId { get; set; }

		public string Message { get; set; }

		public string AssetUrl { get; set; }

		[BsonRepresentation(BsonType.DateTime)]
		public DateTime CreatedAt { get; set; }

		[BsonRepresentation(BsonType.DateTime)]
		public DateTime? SeenAt { get; set; }

		[BsonRepresentation(BsonType.Int32)]
		public NotificationStatus Status { get; set; }
	}
}