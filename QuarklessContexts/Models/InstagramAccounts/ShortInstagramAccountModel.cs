using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuarklessContexts.Models.InstagramAccounts
{
	public class DailyActions
	{
		public DateTime? Date { get; set; }

		[BsonRepresentation(BsonType.Int64)]
		public long NumberOfPostsPosted { get; set; }

		[BsonRepresentation(BsonType.Int64)]
		public long NumberOfCommentsMade { get; set; }

		[BsonRepresentation(BsonType.Int64)]
		public long NumberOfCommentsLiked { get; set; }

		[BsonRepresentation(BsonType.Int64)]
		public long NumberOfPostsLiked { get; set; }

		[BsonRepresentation(BsonType.Int64)]
		public long NumberOfPeopleFollowed { get; set; }

		[BsonRepresentation(BsonType.Int64)]
		public long NumberOfTopicsFollowed { get; set; }
	}
	public class HourlyActions
	{
		public DateTime? Date { get; set; }

		[BsonRepresentation(BsonType.Int64)]
		public long NumberOfPostsPostedHourly { get; set; }

		[BsonRepresentation(BsonType.Int64)]
		public long NumberOfCommentsMadeHourly { get; set; }

		[BsonRepresentation(BsonType.Int64)]
		public long NumberOfCommentsLikedHourly { get; set; }

		[BsonRepresentation(BsonType.Int64)]
		public long NumberOfPostsLikedHourly { get; set; }

		[BsonRepresentation(BsonType.Int64)]
		public long NumberOfPeopleFollowedHourly { get; set; }

		[BsonRepresentation(BsonType.Int64)]
		public long NumberOfTopicsFollowedHourly { get; set; }
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
		AwaitingActionFromUser = 7
	}
	public class AgentSettings
	{
		public int AgentState { get; set; }

		[BsonRepresentation(BsonType.DateTime)]
		public DateTime? LastPurgeCycle { get; set; }
		//public ActionStates ActionStates { get; set; }
	}

	public class ShortInstagramAccountModel
	{
		public string Id { get; set; }
		public string AccountId { get; set; }
		public string Username { get; set; }
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
	}
}
