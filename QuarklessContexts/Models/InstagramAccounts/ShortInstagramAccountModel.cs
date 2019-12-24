using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using QuarklessContexts.Models.Profiles;
using System;

namespace QuarklessContexts.Models.InstagramAccounts
{
	public class ShortInstagramAccountModel
	{
		public string Id { get; set; }
		public string AccountId { get; set; }
		public string Username { get; set; }
		public long? UserId { get; set; }
		public string FullName { get; set; }
		public string ProfilePicture { get; set; }
		public Biography UserBiography { get; set; }
		public Location Location { get; set; }
		public string Email { get; set; }
		public string PhoneNumber { get; set; }
		public int? AgentState { get; set; }
		public long? FollowersCount { get; set; }
		public long? FollowingCount { get; set; }
		public long? TotalPostsCount { get; set; }

		[BsonRepresentation(BsonType.DateTime)]
		public DateTime? LastPurgeCycle { get; set; }

		[BsonRepresentation(BsonType.DateTime)]
		public DateTime? SleepTimeRemaining { get; set; }

		[BsonRepresentation(BsonType.DateTime)]
		public DateTime? DateAdded { get; set; }
		public Limits UserLimits { get; set; }
		public bool? IsBusiness { get; set ;}
		public int? Type { get; set; } 
		public ChallengeCodeRequestResponse ChallengeInfo { get; set; }

	}
}
