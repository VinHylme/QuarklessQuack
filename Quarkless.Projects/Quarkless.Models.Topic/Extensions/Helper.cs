using Quarkless.Models.Common.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Quarkless.Models.Topic.Extensions
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
	}
}
