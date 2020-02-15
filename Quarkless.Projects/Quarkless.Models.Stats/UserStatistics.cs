using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson.Serialization.Attributes;

namespace Quarkless.Models.Stats
{
	public class UserStatistics
	{
		public bool IsLatest { get; set; }
		public int FollowersReceived { get; set; }

		[BsonIgnore]
		public double AverageFollowsReceivedPastHour
		{
			get
			{
				if (UserStatsPastHour == null || !UserStatsPastHour.Any())
					return 0;

				return UserStatsPastHour.Average(_ => _.FollowersReceived);
			}
		}
		[BsonIgnore]
		public double AverageFollowsReceivedPastDay
		{
			get
			{
				if (UserStatsPastDay == null || !UserStatsPastDay.Any())
					return 0;

				return UserStatsPastDay.Average(_ => _.FollowersReceived);
			}
		}

		public int FollowingMade { get; set; }

		[BsonIgnore]
		public double AverageFollowingMadePastHour
		{
			get
			{
				if (UserStatsPastHour == null || !UserStatsPastHour.Any())
					return 0;

				return UserStatsPastHour.Average(_ => _.FollowingMade);
			}
		}
		[BsonIgnore]
		public double AverageFollowingMadePastDay
		{
			get
			{
				if (UserStatsPastDay == null || !UserStatsPastDay.Any())
					return 0;

				return UserStatsPastDay.Average(_ => _.FollowingMade);
			}
		}

		public int LikesReceived { get; set; }
		
		[BsonIgnore]
		public double AverageLikesReceivedPastHour
		{
			get
			{
				if (UserStatsPastHour == null || !UserStatsPastHour.Any())
					return 0;

				return UserStatsPastHour.Average(_ => _.LikesReceived);
			}
		}
		[BsonIgnore]
		public double AverageLikesReceivedMadePastDay
		{
			get
			{
				if (UserStatsPastDay == null || !UserStatsPastDay.Any())
					return 0;

				return UserStatsPastDay.Average(_ => _.LikesReceived);
			}
		}

		public int CommentsReceived { get; set; }

		[BsonIgnore]
		public double AverageCommentsReceivedPastHour
		{
			get
			{
				if (UserStatsPastHour == null || !UserStatsPastHour.Any())
					return 0;

				return UserStatsPastHour.Average(_ => _.CommentsReceived);
			}
		}
		[BsonIgnore]
		public double AverageCommentsReceivedMadePastDay
		{
			get
			{
				if (UserStatsPastDay == null || !UserStatsPastDay.Any())
					return 0;

				return UserStatsPastDay.Average(_ => _.CommentsReceived);
			}
		}

		public DateTime LastUpdateTime { get; set; }
		public List<UserStatistics> UserStatsPastHour { get; set; }
		public List<UserStatistics> UserStatsPastDay { get; set; }
	}
}