using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using QuarklessContexts.Models.Profiles;
using System;
using System.Collections;
using System.Collections.Generic;

namespace QuarklessContexts.Models.InstagramAccounts
{
	public class DailyActions
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
	}
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
	}
	public class ActionStates
	{
		public HourlyActions Hourly { get; set; }
		public DailyActions Daily { get; set; }
	}
	public enum AgentState
	{
		NotStarted  = 0,
		Running = 1,
		Stopped = 2,
		Sleeping = 3,
		DeepSleep = 4,
		Blocked = 5,
		Challenge = 6,
		AwaitingActionFromUser = 7,
	}
	public class Biography
	{
		public string Text { get; set; }
		public IList<string> Hashtags { get; set; }
	}
	public class Limits
	{
		public DailyActions DailyLimits { get; set; }
		public HourlyActions HourlyLimits { get; set; }
	}
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
