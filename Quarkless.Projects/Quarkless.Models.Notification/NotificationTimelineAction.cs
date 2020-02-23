using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Common.Models.Resolver;
using Quarkless.Models.Notification.Enums;

namespace Quarkless.Models.Notification
{
	public class NotificationTimelineAction
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		public string _id { get; set; }

		public NotificationObject Notification { get; set; }

		[BsonRepresentation(BsonType.Int32)]
		public TimelineEventItemStatus TimelineStatus { get; set; }

		[BsonRepresentation(BsonType.Int32)]
		public ActionType ActionType { get; set; }
		public int ResponseType { get; set; }
		public string ResponseMessage { get; set; }
		public MediaShort Media { get; set; }
	}
}