using Quarkless.Repository.RedisContext.Models;

namespace Quarkless.Repository.RedisContext.Extensions
{
	public static class KeyFormatter
	{
		public static string FormatKey(string userId, RedisKeys.HashtagGrowKeys hashtagGrowKey)
		{
			var formatTemplate = $"HashtagGrow:{hashtagGrowKey.ToString()}:({userId}:{hashtagGrowKey.ToString()})";
			return formatTemplate;
		}
		public static string FormatKeyVal(string value, RedisKeys.HashtagGrowKeys hashtagGrowKey)
		{
			return $"HashtagGrow:{hashtagGrowKey.ToString()}:{value}";
		}
	}
}