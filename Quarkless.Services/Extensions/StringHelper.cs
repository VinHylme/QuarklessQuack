using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quarkless.Services.Extensions
{
	public static class StringHelper
	{
		public static int Similarity(this string s, string t)
		{
			if (string.IsNullOrEmpty(s))
			{
				if (string.IsNullOrEmpty(t))
					return 0;
				return t.Length;
			}

			if (string.IsNullOrEmpty(t))
			{
				return s.Length;
			}

			int n = s.Length;
			int m = t.Length;
			int[,] d = new int[n + 1, m + 1];

			// initialize the top and right of the table to 0, 1, 2, ...
			for (int i = 0; i <= n; d[i, 0] = i++) ;
			for (int j = 1; j <= m; d[0, j] = j++) ;

			for (int i = 1; i <= n; i++)
			{
				for (int j = 1; j <= m; j++)
				{
					int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
					int min1 = d[i - 1, j] + 1;
					int min2 = d[i, j - 1] + 1;
					int min3 = d[i - 1, j - 1] + cost;
					d[i, j] = Math.Min(Math.Min(min1, min2), min3);
				}
			}
			return d[n, m];
		}
		public static string JoinEvery(this IEnumerable<string> @strings, string seperator, int every)
		{
			string result = string.Empty;

			for(int i = 0 ; i < @strings.Count(); i++)
			{
				if (i % every == 0 && i!=0)
				{
					for(int j = Math.Abs(i-every); j < (i-every)+every; j++)
						result += @strings.ElementAt(j) + " ";
					result+=seperator;
				}
			}

			return result;
		}
	}
}
