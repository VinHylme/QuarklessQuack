using System;
using Quarkless.Base.Auth.Common.Models.Enums;
using Quarkless.Base.InstagramAccounts.Models;

namespace Quarkless.Base.Actions.Logic.Limits
{
	public static class ActionLimits
	{
		#region Action Constants
		private const int DAYS_CONSIDERED_WARMING_UP = 14;

		public const int MAX_DAILY_WATCH_STORY_ACTION = 150000;
		public const int MAX_HOURLY_WATCH_STORY_ACTION = 500;

		public const int MAX_DAILY_REACT_STORY_ACTION = 150;
		public const int MAX_HOURLY_REACT_STORY_ACTION = 25;

		public const int MAX_DAILY_DM_ACTION = 200;
		public const int MAX_HOURLY_DM_ACTION = 12;

		public const int MAX_DAILY_LIKE_ACTION = 900;
		public const int MAX_HOURLY_LIKE_ACTION = 55;

		public const int MAX_DAILY_FOLLOW_ACTION = 225;
		public const int MAX_HOURLY_FOLLOW_ACTION = 40;

		public const int MAX_DAILY_POST_MEDIA_ACTION = 24;
		public const int MAX_HOURLY_POST_MEDIA_ACTION = 3;

		public const int MAX_DAILY_COMMENT_ACTION = 500;
		public const int MAX_HOURLY_COMMENT_ACTION = 55;

		public const int MIN_DAILY_DM_ACTION = 10;
		public const int MIN_HOURLY_DM_ACTION = 1;

		public const int MIN_DAILY_LIKE_ACTION = 20;
		public const int MIN_HOURLY_LIKE_ACTION = 4;

		public const int MIN_DAILY_FOLLOW_ACTION = 12;
		public const int MIN_HOURLY_FOLLOW_ACTION = 2;

		public const int MIN_DAILY_POST_MEDIA_ACTION = 2;
		public const int MIN_HOURLY_POST_MEDIA_ACTION = 1;

		public const int MIN_DAILY_COMMENT_ACTION = 4;
		public const int MIN_HOURLY_COMMENT_ACTION = 1;

		public const int MIN_DAILY_WATCH_STORY_ACTION = 1000;
		public const int MIN_HOURLY_WATCH_STORY_ACTION = 100;

		public const int MIN_DAILY_REACT_STORY_ACTION = 12;
		public const int MIN_HOURLY_REACT_STORY_ACTION = 1;

		#endregion

		public static InstagramAccounts.Models.Limits Empty => new InstagramAccounts.Models.Limits
		{
			DailyLimits = new DailyActions
			{
				SendMessageLimit = 0,
				CreateCommentLimit = 0,
				CreatePostLimit = 0,
				FollowPeopleLimit = 0,
				FollowTopicLimit = 0,
				LikeCommentLimit = 0,
				LikePostLimit = 0,
				WatchStoryLimit = 0,
				ReactStoryLimit = 0
			},
			HourlyLimits = new HourlyActions
			{
				SendMessageLimit = 0,
				CreatePostLimit = 0,
				CreateCommentLimit = 0,
				FollowPeopleLimit = 0,
				FollowTopicLimit = 0,
				LikeCommentLimit = 0,
				LikePostLimit = 0,
				WatchStoryLimit = 0,
				ReactStoryLimit = 0
			}
		};
		public static InstagramAccounts.Models.Limits FullAuth(int minusFromEach = 0) => new InstagramAccounts.Models.Limits
		{
			DailyLimits = new DailyActions
			{
				SendMessageLimit = MAX_DAILY_DM_ACTION - minusFromEach,
				CreateCommentLimit = MAX_DAILY_COMMENT_ACTION - minusFromEach,
				CreatePostLimit = MAX_DAILY_POST_MEDIA_ACTION - minusFromEach,
				FollowPeopleLimit = MAX_DAILY_FOLLOW_ACTION - minusFromEach,
				FollowTopicLimit = MAX_DAILY_FOLLOW_ACTION - minusFromEach,
				LikeCommentLimit = MAX_DAILY_LIKE_ACTION - minusFromEach,
				LikePostLimit = MAX_DAILY_LIKE_ACTION - minusFromEach,
				WatchStoryLimit = MAX_DAILY_WATCH_STORY_ACTION - (minusFromEach * 10),
				ReactStoryLimit = MAX_DAILY_REACT_STORY_ACTION - minusFromEach
			},
			HourlyLimits = new HourlyActions
			{
				SendMessageLimit = MAX_HOURLY_DM_ACTION,
				CreatePostLimit = MAX_HOURLY_POST_MEDIA_ACTION,
				CreateCommentLimit = MAX_HOURLY_COMMENT_ACTION,
				FollowPeopleLimit = MAX_HOURLY_FOLLOW_ACTION,
				FollowTopicLimit = MAX_HOURLY_FOLLOW_ACTION,
				LikeCommentLimit = MAX_HOURLY_LIKE_ACTION,
				LikePostLimit = MAX_HOURLY_LIKE_ACTION,
				WatchStoryLimit = MAX_HOURLY_WATCH_STORY_ACTION,
				ReactStoryLimit = MAX_HOURLY_REACT_STORY_ACTION
			}
		};
		public static InstagramAccounts.Models.Limits Min => new InstagramAccounts.Models.Limits
		{
			DailyLimits = new DailyActions
			{
				SendMessageLimit = MIN_DAILY_DM_ACTION,
				CreateCommentLimit = MIN_DAILY_COMMENT_ACTION,
				CreatePostLimit = MIN_DAILY_POST_MEDIA_ACTION,
				FollowPeopleLimit = MIN_DAILY_FOLLOW_ACTION,
				FollowTopicLimit = MIN_DAILY_FOLLOW_ACTION,
				LikeCommentLimit = MIN_DAILY_LIKE_ACTION,
				LikePostLimit = MIN_DAILY_LIKE_ACTION,
				WatchStoryLimit = MIN_DAILY_WATCH_STORY_ACTION,
				ReactStoryLimit = MIN_DAILY_REACT_STORY_ACTION
			},
			HourlyLimits = new HourlyActions
			{
				SendMessageLimit = MIN_HOURLY_DM_ACTION,
				CreatePostLimit = MIN_HOURLY_POST_MEDIA_ACTION,
				CreateCommentLimit = MIN_HOURLY_COMMENT_ACTION,
				FollowPeopleLimit = MIN_HOURLY_FOLLOW_ACTION,
				FollowTopicLimit = MIN_HOURLY_FOLLOW_ACTION,
				LikeCommentLimit = MIN_HOURLY_LIKE_ACTION,
				LikePostLimit = MIN_HOURLY_LIKE_ACTION,
				WatchStoryLimit = MIN_HOURLY_WATCH_STORY_ACTION,
				ReactStoryLimit = MIN_HOURLY_REACT_STORY_ACTION
			}
		};
		public static InstagramAccounts.Models.Limits SetLimits(AuthTypes authType, DateTime accountCreationDate)
		{
			var warmingUpDateLimit = DateTime.UtcNow.AddDays(DAYS_CONSIDERED_WARMING_UP);
			var isWarmingUp = false;
			var scaleFactor = 0.0;

			var daysPast = (int) Math.Abs((accountCreationDate - DateTime.UtcNow).TotalDays);

			if (daysPast < DAYS_CONSIDERED_WARMING_UP)
			{
				isWarmingUp = true;
				scaleFactor = (double) daysPast / DAYS_CONSIDERED_WARMING_UP;

				if (scaleFactor <= 0)
				{
					scaleFactor = 0.125;
				}
			}

			var maxLimitsForUser = LimitsByAuth(authType);

			if (isWarmingUp)
			{
				return new InstagramAccounts.Models.Limits
				{
					DailyLimits = new DailyActions
					{
						SendMessageLimit = (int) (scaleFactor * maxLimitsForUser.DailyLimits.SendMessageLimit / MIN_DAILY_DM_ACTION * MIN_DAILY_DM_ACTION),
						CreateCommentLimit = (int) (scaleFactor * maxLimitsForUser.DailyLimits.CreateCommentLimit / MIN_DAILY_COMMENT_ACTION * MIN_DAILY_COMMENT_ACTION),
						CreatePostLimit = (int) (scaleFactor * maxLimitsForUser.DailyLimits.CreatePostLimit / MIN_DAILY_POST_MEDIA_ACTION * MIN_DAILY_POST_MEDIA_ACTION),
						FollowPeopleLimit = (int) (scaleFactor * maxLimitsForUser.DailyLimits.FollowPeopleLimit / MIN_DAILY_FOLLOW_ACTION * MIN_DAILY_FOLLOW_ACTION),
						FollowTopicLimit = (int) (scaleFactor * maxLimitsForUser.DailyLimits.FollowTopicLimit / MIN_DAILY_FOLLOW_ACTION * MIN_DAILY_FOLLOW_ACTION),
						LikeCommentLimit = (int) (scaleFactor * maxLimitsForUser.DailyLimits.LikeCommentLimit / MIN_DAILY_LIKE_ACTION * MIN_DAILY_LIKE_ACTION),
						LikePostLimit = (int) (scaleFactor * maxLimitsForUser.DailyLimits.LikePostLimit / MIN_DAILY_LIKE_ACTION * MIN_DAILY_LIKE_ACTION),
						WatchStoryLimit = (int) (scaleFactor * maxLimitsForUser.DailyLimits.WatchStoryLimit / MIN_DAILY_WATCH_STORY_ACTION * MIN_DAILY_WATCH_STORY_ACTION),
						ReactStoryLimit = (int)(scaleFactor * maxLimitsForUser.DailyLimits.ReactStoryLimit / MIN_DAILY_REACT_STORY_ACTION * MIN_DAILY_REACT_STORY_ACTION),
					},
					HourlyLimits = new HourlyActions
					{
						SendMessageLimit = MIN_HOURLY_DM_ACTION,
						CreatePostLimit = MIN_HOURLY_POST_MEDIA_ACTION,
						CreateCommentLimit = (int) (scaleFactor * maxLimitsForUser.HourlyLimits.CreateCommentLimit / MIN_HOURLY_COMMENT_ACTION * MIN_HOURLY_COMMENT_ACTION),
						FollowPeopleLimit = (int) (scaleFactor * maxLimitsForUser.HourlyLimits.FollowPeopleLimit / MIN_HOURLY_FOLLOW_ACTION * MIN_HOURLY_FOLLOW_ACTION),
						FollowTopicLimit = (int) (scaleFactor * maxLimitsForUser.HourlyLimits.FollowTopicLimit / MIN_HOURLY_FOLLOW_ACTION * MIN_HOURLY_FOLLOW_ACTION),
						LikeCommentLimit = (int) (scaleFactor * maxLimitsForUser.HourlyLimits.LikeCommentLimit / MIN_HOURLY_LIKE_ACTION * MIN_HOURLY_LIKE_ACTION),
						LikePostLimit = (int) (scaleFactor * maxLimitsForUser.HourlyLimits.LikePostLimit / MIN_HOURLY_LIKE_ACTION * MIN_HOURLY_LIKE_ACTION),
						WatchStoryLimit = (int) (scaleFactor * maxLimitsForUser.HourlyLimits.WatchStoryLimit / MIN_HOURLY_WATCH_STORY_ACTION * MIN_HOURLY_WATCH_STORY_ACTION),
						ReactStoryLimit = MIN_DAILY_REACT_STORY_ACTION
					}
				};
			}

			return maxLimitsForUser;
		}

		public static InstagramAccounts.Models.Limits LimitsByAuth(AuthTypes authTypes)
		{
			switch (authTypes)
			{
				case AuthTypes.Expired:
					return Empty;
				case AuthTypes.TrialUsers:
					return new InstagramAccounts.Models.Limits
					{
						DailyLimits = new DailyActions
						{
							SendMessageLimit = 10,
							CreateCommentLimit = 60,
							CreatePostLimit = 4,
							FollowPeopleLimit = 40,
							FollowTopicLimit = 40,
							LikeCommentLimit = 250,
							LikePostLimit = 250,
							WatchStoryLimit = 5000,
							ReactStoryLimit = 25
						},
						HourlyLimits = new HourlyActions
						{
							SendMessageLimit = 1,
							CreatePostLimit = 1,
							CreateCommentLimit = 10,
							FollowPeopleLimit = 15,
							FollowTopicLimit = 15,
							LikeCommentLimit = 40,
							LikePostLimit = 40,
							WatchStoryLimit = 150,
							ReactStoryLimit = 1
						}
					};
				case AuthTypes.BasicUsers:
					return new InstagramAccounts.Models.Limits
					{
						DailyLimits = new DailyActions
						{
							SendMessageLimit = 24,
							CreateCommentLimit = 100,
							CreatePostLimit = 8,
							FollowPeopleLimit = 80,
							FollowTopicLimit = 80,
							LikeCommentLimit = 500,
							LikePostLimit = 500,
							WatchStoryLimit = 75000,
							ReactStoryLimit = 35
						},
						HourlyLimits = new HourlyActions
						{
							SendMessageLimit = 3,
							CreatePostLimit = 2,
							CreateCommentLimit = 15,
							FollowPeopleLimit = 25,
							FollowTopicLimit = 25,
							LikeCommentLimit = 80,
							LikePostLimit = 80,
							WatchStoryLimit = 300,
							ReactStoryLimit = 5
						}
					};
				case AuthTypes.PremiumUsers:
					return new InstagramAccounts.Models.Limits
					{
						DailyLimits = new DailyActions
						{
							SendMessageLimit = 125,
							CreateCommentLimit = 280,
							CreatePostLimit = 15,
							FollowPeopleLimit = 125,
							FollowTopicLimit = 125,
							LikeCommentLimit = 650,
							LikePostLimit = 650,
							WatchStoryLimit = 100000,
							ReactStoryLimit = 90
						},
						HourlyLimits = new HourlyActions
						{
							SendMessageLimit = 7,
							CreatePostLimit = 3,
							CreateCommentLimit = 42,
							FollowPeopleLimit = 42,
							FollowTopicLimit = 42,
							LikeCommentLimit = 90,
							LikePostLimit = 90,
							WatchStoryLimit = 400,
							ReactStoryLimit = 10
						}
					};
				case AuthTypes.EnterpriseUsers:
					return FullAuth(10);
				case AuthTypes.Admin:
					return FullAuth();
				default:
					return Empty;
			}
		}

	}
}
