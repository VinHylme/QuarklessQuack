using System;
using System.ComponentModel;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using QuarklessContexts.Enums;

namespace QuarklessContexts.Models.TimelineLoggingRepository
{
	public enum TimelineEventStatus
	{
		[Description("not started")]
		NotStarted = 0,
		[Description("failed")]
		Failed = 1,
		[Description("sucess")]
		Success = 2,
		[Description("feedback")]
		FeedbackRequired = 3,
		[Description("Internal Server Error")]
		ServerError
	}
	public class TimelineEventLog
	{
		[BsonId] [BsonRepresentation(BsonType.ObjectId)]
		public string _id { get; set; }

		[BsonRepresentation(BsonType.ObjectId)]
		public string InstagramAccountID { get; set;  }
		public string AccountID { get; set;  }

		[BsonRepresentation(BsonType.Int32)]
		public TimelineEventStatus Status {get ; set; }

		[BsonRepresentation(BsonType.Int32)]
		public ActionType ActionType { get; set; }
		public string Message { get; set; }
		public Exception Exception { get; set ; }
		public string Request { get ; set;  }
		public string Response { get; set; }
		public int Level { get; set;  }
		public DateTime DateAdded { get; set; } = DateTime.UtcNow;
	}
}
