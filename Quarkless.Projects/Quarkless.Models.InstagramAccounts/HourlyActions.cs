using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Quarkless.Models.InstagramAccounts
{
	public class HourlyActions
	{
		[BsonRepresentation(BsonType.Int32)]
		public long CreatePostLimit { get; set; }

		[BsonRepresentation(BsonType.Int32)]
		public long SendMessageLimit { get; set; }

		[BsonRepresentation(BsonType.Int32)]
		public long CreateCommentLimit { get; set; }

		[BsonRepresentation(BsonType.Int32)]
		public long LikeCommentLimit { get; set; }

		[BsonRepresentation(BsonType.Int32)]
		public long LikePostLimit { get; set; }

		[BsonRepresentation(BsonType.Int32)]
		public long FollowPeopleLimit { get; set; }

		[BsonRepresentation(BsonType.Int32)]
		public long FollowTopicLimit { get; set; }

		[BsonRepresentation(BsonType.Int32)]
		public long WatchStoryLimit { get; set; }
	}
}