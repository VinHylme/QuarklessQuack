using InstagramApiSharp.Classes;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace QuarklessContexts.Models.InstagramAccounts
{
	public class InstagramAccountModel
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		public string _id { get; set; }
		public string AccountId { get; set; }
		public StateData State { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
		public int? FollowersCount { get; set; }
		public int? FollowingCount { get; set; }
		public int? TotalPostsCount { get; set; }
		public int? Type { get; set; } //0 = normal account, 1 = learner account
		public int? TotalLikes { get; set; }
		public string Device { get; set; }
		public bool? AgentState { get; set; }

	}
}
