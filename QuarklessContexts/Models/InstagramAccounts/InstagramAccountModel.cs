using InstagramApiSharp.Classes;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

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
		public long? FollowersCount { get; set; }
		public long? FollowingCount { get; set; }
		public long? TotalPostsCount { get; set; }
		public int? Type { get; set; } //0 = normal account, 1 = learner account
		public string Device { get; set; }
		public int? AgentState { get; set; }
		[BsonRepresentation(BsonType.DateTime)]
		public DateTime? LastPurgeCycle { get; set; }
		public DateTime? DateAdded { get; set; }
	}
}
