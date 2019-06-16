using System;
using System.IO;
using System.Text.RegularExpressions;
using WDict = System.Collections.Generic.Dictionary<string, uint>;
using TDict = System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, uint>>;
using QuarklessContexts.Extensions;
using System.Text;
using System.Linq;

namespace QuarklessLogic.Handlers.TextGeneration
{
	public class TextGeneration : ITextGeneration
	{
		//TODO: ADD AI LATER, FOR NOW JUST USING MARKOV ALGO
		public string MarkovTextGenerator(string filePath, int limit, int size, bool exact = false)
		{
			if (!File.Exists(filePath)) { Console.WriteLine("Input file doesn't exist"); return null; }

			string s = Regex.Replace(File.ReadAllText(filePath), @"\s+", " ").TrimEnd(' ');
			TDict t = MarkovHelper.BuildTDict(s, size);
			return MarkovHelper.BuildString(t, limit, exact).TrimEnd(' ');
		}

		public string MarkovTextGenerator(string filePath, int type, string topic, string language, int size, int limit)
		{
			if (type < 0 && type > 2) throw new Exception("invalid type");
			string joinedPath = string.Format(filePath,
				type == 0 ? "_comments" : type == 1 ? "_captions" : type == 2 ? "_bios" : null);
			var reader = File.ReadAllLines(joinedPath)
				.Skip(1)
				.Select(a => a.Split("\","))
				.Where(c => c.Length > 2)
				.Select(v => new { Text = v[0].Replace("\"", ""), Topic = v[1].Replace("\"", ""), Language = v[2].Replace("\"", "") })
				.Where(_ => _.Topic.ToLower().Contains(topic) && _.Language.Contains(language)).ToList();

			if(reader.Count<=0) return null;
			string s = Regex.Replace(string.Join(',', reader.Select(sa => sa.Text)), @"\s+", " ").TrimEnd(' ');
			TDict t = MarkovHelper.BuildTDict(s, size);
			return MarkovHelper.BuildString(t, limit, false).TrimEnd(' ');
		}

	}
}
