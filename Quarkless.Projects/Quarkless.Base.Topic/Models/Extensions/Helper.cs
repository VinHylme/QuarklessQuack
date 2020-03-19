using InstagramApiSharp.Classes.Models;
using System.Collections.Generic;
using System.Linq;
using Quarkless.Models.Common.Extensions;
using Quarkless.Models.Common.Models.Topic;

namespace Quarkless.Base.Topic.Models.Extensions
{
	public static class Helper
	{
		public static int ComputeTopicHashCode(this IEnumerable<CTopic> topics)
		{
			var total = 0;
			foreach (var topic in topics.Take(2).Select(_ => _.Name.ToByteArray()))
			{
				total ^= topic.ComputeHash();
			}
			return total;
		}

		public static int GetRarity(this InstaHashtag hashtag)
		{
			if (hashtag.MediaCount <= 5000)
			{
				return 0;
			}

			if (hashtag.MediaCount <= 500000 && hashtag.MediaCount > 5000)
			{
				return 2;
			}

			return hashtag.MediaCount > 500000 ? 1 : 0;
		}
	}
}
