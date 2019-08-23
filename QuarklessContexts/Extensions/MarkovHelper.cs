using System.Collections.Generic;
using System.Text;
using WDict = System.Collections.Generic.Dictionary<string, uint>;
using TDict = System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, uint>>;
using System.Linq;
using System;
using System.Security.Cryptography;
using System.Globalization;

namespace QuarklessContexts.Extensions
{
	public static class MarkovHelper
	{
		public static string MapLanguages(this string language)
		{
			var all = CultureInfo.GetCultures(CultureTypes.AllCultures);
			var shortv = all.Where(s => s.EnglishName.ToLower() == language.ToLower()).Select(s => s.Name).SingleOrDefault();
			return !string.IsNullOrEmpty(shortv) ? shortv : null;
		}
		public static TDict BuildTDict(string s, int size)
		{
			TDict t = new TDict();
			string prev = "";
			foreach (string word in Chunk(s, size))
			{
				if (t.ContainsKey(prev))
				{
					WDict w = t[prev];
					if (w.ContainsKey(word))
						w[word] += 1;
					else
						w.Add(word, 1);
				}
				else
					t.Add(prev, new WDict() { { word, 1 } });

				prev = word;
			}

			return t;
		}

		public static string[] Chunk(string s, int size)
		{
			string[] ls = s.Split(' ');
			List<string> chunk = new List<string>();

			for (int i = 0; i < ls.Length - size; ++i)
			{
				StringBuilder sb = new StringBuilder();
				sb.Append(ls.Skip(i).Take(size).Aggregate((w, k) => w + " " + k));
				chunk.Add(sb.ToString());
			}

			return chunk.ToArray();
		}

		public static string BuildString(TDict t, int len, bool exact)
		{
			string last;
			List<string> ucStr = new List<string>();
			StringBuilder sb = new StringBuilder();

			foreach (string word in t.Keys.Skip(1))
			{
				if (char.IsUpper(word.First()))
					ucStr.Add(word);
			}

			if (ucStr.Count > 0)
				sb.Append(ucStr.ElementAt(SecureRandom.Next(0, ucStr.Count)));

			last = sb.ToString();
			sb.Append(" ");

			WDict w = new WDict();

			for (uint i = 0; i < len; ++i)
			{
				if (t.ContainsKey(last))
					w = t[last];
				else
					w = t[""];

				last = MarkovHelper.Choose(w);
				sb.Append(last.Split(' ').Last()).Append(" ");
			}

			if (!exact)
			{
				while (last.Last() != '.')
				{
					if (t.ContainsKey(last))
						w = t[last];
					else
						w = t[""];

					last = MarkovHelper.Choose(w);
					sb.Append(last.Split(' ').Last()).Append(" ");
				}
			}

			return sb.ToString();
		}

		private static string Choose(WDict w)
		{
			long total = w.Sum(t => t.Value);

			while (true)
			{
				int i = SecureRandom.Next(0, w.Count);
				double c = SecureRandom.NextDouble();
				System.Collections.Generic.KeyValuePair<string, uint> k = w.ElementAt(i);

				if (c < (double)k.Value / total)
					return k.Key;
			}
		}
	}
}
