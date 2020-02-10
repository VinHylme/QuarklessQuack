using System;
using Quarkless.Models.Auth.Enums;
using Quarkless.Models.InstagramAccounts;

namespace Quarkless.Logic.Actions.Limits
{
	public static class ActionLimits
	{
		#region Action Constants
		private const int DAYS_CONSIDERED_WARMING_UP = 14;

		public const int MAX_DAILY_WATCH_STORY_ACTION = 150000;
		public const int MAX_HOURLY_WATCH_STORY_ACTION = 500;

		public const int MAX_DAILY_DM_ACTION = 200;
		public const int MAX_HOURLY_DM_ACTION = 12;

		public const int MAX_DAILY_LIKE_ACTION = 900;
		public const int MAX_HOURLY_LIKE_ACTION = 55;

		public const int MAX_DAILY_FOLLOW_ACTION = 225;
		public const int MAX_HOURLY_FOLLOW_ACTION = 40;

		public const int MAX_DAILY_POST_MEDIA_ACTION = 24;
		public const int MAX_HOURLY_POST_MEDIA_ACTION = 2;

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
		#endregion

		public static Models.InstagramAccounts.Limits Empty => new Models.InstagramAccounts.Limits
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
				WatchStoryLimit = 0
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
				WatchStoryLimit = 0
			}
		};
		public static Models.InstagramAccounts.Limits FullAuth(int minusFromEach = 0) => new Models.InstagramAccounts.Limits
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
				WatchStoryLimit = MAX_DAILY_WATCH_STORY_ACTION - (minusFromEach * 10)
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
				WatchStoryLimit = MAX_HOURLY_WATCH_STORY_ACTION
			}
		};
		public static Models.InstagramAccounts.Limits Min => new Models.InstagramAccounts.Limits
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
				WatchStoryLimit = MIN_DAILY_WATCH_STORY_ACTION
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
				WatchStoryLimit = MIN_HOURLY_WATCH_STORY_ACTION
			}
		};
		public static Models.InstagramAccounts.Limits SetLimits(AuthTypes authType, DateTime accountCreationDate)
		{
			var warmingUpDateLimit = DateTime.UtcNow.AddDays(DAYS_CONSIDERED_WARMING_UP);
			var isWarmingUp = false;
			var scaleFactor = 0.0;

			var daysPast = (int) Math.Abs((accountCreationDate - DateTime.UtcNow).TotalDays);

			if (daysPast <= DAYS_CONSIDERED_WARMING_UP)
			{
				isWarmingUp = true;
				scaleFactor = (double) daysPast / DAYS_CONSIDERED_WARMING_UP;

				if (scaleFactor <= 0)
				{
					scaleFactor = 0.05;
				}
			}

			var maxLimitsForUser = LimitsByAuth(authType);

			if (isWarmingUp)
			{
				return new Models.InstagramAccounts.Limits
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
						WatchStoryLimit = (int) (scaleFactor * maxLimitsForUser.DailyLimits.WatchStoryLimit / MIN_DAILY_WATCH_STORY_ACTION * MIN_DAILY_WATCH_STORY_ACTION)

					},
					HourlyLimits = new HourlyActions
					{
						SendMessageLimit = (int) (scaleFactor * maxLimitsForUser.HourlyLimits.SendMessageLimit / MIN_HOURLY_DM_ACTION * MIN_HOURLY_DM_ACTION),
						CreatePostLimit = (int) (scaleFactor * maxLimitsForUser.HourlyLimits.CreatePostLimit / MIN_HOURLY_POST_MEDIA_ACTION * MIN_HOURLY_POST_MEDIA_ACTION),
						CreateCommentLimit = (int) (scaleFactor * maxLimitsForUser.HourlyLimits.CreateCommentLimit / MIN_HOURLY_COMMENT_ACTION * MIN_HOURLY_COMMENT_ACTION),
						FollowPeopleLimit = (int) (scaleFactor * maxLimitsForUser.HourlyLimits.FollowPeopleLimit / MIN_HOURLY_FOLLOW_ACTION * MIN_HOURLY_FOLLOW_ACTION),
						FollowTopicLimit = (int) (scaleFactor * maxLimitsForUser.HourlyLimits.FollowTopicLimit / MIN_HOURLY_FOLLOW_ACTION * MIN_HOURLY_FOLLOW_ACTION),
						LikeCommentLimit = (int) (scaleFactor * maxLimitsForUser.HourlyLimits.LikeCommentLimit / MIN_HOURLY_LIKE_ACTION * MIN_HOURLY_LIKE_ACTION),
						LikePostLimit = (int) (scaleFactor * maxLimitsForUser.HourlyLimits.LikePostLimit / MIN_HOURLY_LIKE_ACTION * MIN_HOURLY_LIKE_ACTION),
						WatchStoryLimit = (int) (scaleFactor * maxLimitsForUser.HourlyLimits.WatchStoryLimit / MIN_HOURLY_WATCH_STORY_ACTION * MIN_HOURLY_WATCH_STORY_ACTION)
					}
				};
			}

			return maxLimitsForUser;
		}

		public static Models.InstagramAccounts.Limits LimitsByAuth(AuthTypes authTypes)
		{
			switch (authTypes)
			{
				case AuthTypes.Expired:
					return Empty;
				case AuthTypes.TrialUsers:
					return new Models.InstagramAccounts.Limits
					{
						DailyLimits = new DailyActions
						{
							SendMessageLimit = 10,
							CreateCommentLimit = 60,
							CreatePostLimit = 4,
							FollowPeopleLimit = 40,
							FollowTopicLimit = 40,
							LikeCommentLimit = 100,
							LikePostLimit = 100,
							WatchStoryLimit = 5000
						},
						HourlyLimits = new HourlyActions
						{
							SendMessageLimit = 1,
							CreatePostLimit = 1,
							CreateCommentLimit = 15,
							FollowPeopleLimit = 15,
							FollowTopicLimit = 15,
							LikeCommentLimit = 15,
							LikePostLimit = 15,
							WatchStoryLimit = 150
						}
					};
				case AuthTypes.BasicUsers:
					return new Models.InstagramAccounts.Limits
					{
						DailyLimits = new DailyActions
						{
							SendMessageLimit = 24,
							CreateCommentLimit = 100,
							CreatePostLimit = 8,
							FollowPeopleLimit = 80,
							FollowTopicLimit = 80,
							LikeCommentLimit = 200,
							LikePostLimit = 200,
							WatchStoryLimit = 75000
						},
						HourlyLimits = new HourlyActions
						{
							SendMessageLimit = 3,
							CreatePostLimit = 2,
							CreateCommentLimit = 30,
							FollowPeopleLimit = 30,
							FollowTopicLimit = 30,
							LikeCommentLimit = 30,
							LikePostLimit = 30,
							WatchStoryLimit = 300
						}
					};
				case AuthTypes.PremiumUsers:
					return new Models.InstagramAccounts.Limits
					{
						DailyLimits = new DailyActions
						{
							SendMessageLimit = 125,
							CreateCommentLimit = 280,
							CreatePostLimit = 15,
							FollowPeopleLimit = 125,
							FollowTopicLimit = 125,
							LikeCommentLimit = 600,
							LikePostLimit = 600,
							WatchStoryLimit = 100000
						},
						HourlyLimits = new HourlyActions
						{
							SendMessageLimit = 7,
							CreatePostLimit = 3,
							CreateCommentLimit = 42,
							FollowPeopleLimit = 42,
							FollowTopicLimit = 42,
							LikeCommentLimit = 42,
							LikePostLimit = 42,
							WatchStoryLimit = 400
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
