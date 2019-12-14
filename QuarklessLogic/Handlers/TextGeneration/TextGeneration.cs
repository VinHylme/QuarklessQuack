using System;
using System.IO;
using System.Text.RegularExpressions;
using QuarklessContexts.Extensions;
using System.Linq;
using System.Threading.Tasks;
using MoreLinq;
using QuarklessLogic.ServicesLogic.CorpusLogic;

namespace QuarklessLogic.Handlers.TextGeneration
{
	//TODO ADD AI LATER, FOR NOW JUST USING MARKOV ALGO
	public class TextGeneration : ITextGeneration
	{
		private readonly ICommentCorpusLogic _commentCorpusLogic;
		private readonly IMediaCorpusLogic _mediaCorpusLogic;
		public TextGeneration(ICommentCorpusLogic commentCorpusLogic, IMediaCorpusLogic mediaCorpusLogic)
		{
			_commentCorpusLogic = commentCorpusLogic;
			_mediaCorpusLogic = mediaCorpusLogic;
		}

		#region MARKOV TEXT GENERATION 
		public string MarkovTextGenerator(string filePath, int limit, int size, bool exact = false)
		{
			if (!File.Exists(filePath)) { Console.WriteLine("Input file doesn't exist"); return null; }
			var s = Regex.Replace(File.ReadAllText(filePath), @"\s+", " ").TrimEnd(' ');
			var t = MarkovHelper.BuildTDict(s, size);
			return MarkovHelper.BuildString(t, limit, exact).TrimEnd(' ');
		}
		public async Task<string> MarkovIt(int type, string topic, string language, int limit)
		{
			if (type < 0 && type > 2) throw new Exception("invalid type");
			const int takeSize = 15;
			switch (type)
			{
				case 0:
					var data = (await _commentCorpusLogic
						.GetComments(topic.OnlyWords(), language.MapLanguages().OnlyWords(),333))
						.DistinctBy(x=>x.Comment);
					if (data == null) return null;
					data = data.TakeAny(takeSize);
					
					var comments = data.Select(sa =>
					{
						var pos = sa.Comment.LastIndexOf(',');
						return pos > 0 ? sa.Comment.Remove(pos, 1) : sa.Comment;
					});

					var dataComment = Regex.Replace(string.Join(',', comments), @"\s+", " ").TrimEnd(' ');
					var dCommentDict = MarkovHelper.BuildTDict(dataComment, takeSize/2);
					return MarkovHelper.BuildString(dCommentDict, limit, true).TrimEnd(' ').Replace(","," ");
				case 1:
					var metaMedia = (await _mediaCorpusLogic
						.GetMedias(topic.OnlyWords(), language.MapLanguages().OnlyWords(), 333))
						.DistinctBy(x => x.Caption);

					metaMedia = metaMedia.TakeAny(takeSize);
					var captions = metaMedia.Select(sa =>
					{
						var pos = sa.Caption.LastIndexOf(',');
						return pos > 0 ? sa.Caption.Remove(pos, 1) : sa.Caption;
					});
					var dataMedia = Regex.Replace(string.Join(',', captions), @"\s+", " ").TrimEnd(' ');
					var dMediaDict = MarkovHelper.BuildTDict(dataMedia, takeSize/2);
					return MarkovHelper.BuildString(dMediaDict, limit, true).TrimEnd(' ').Replace(","," ");
				case 2:
					break;
			}
			return null;
		}
		public string MarkovTextGenerator(string filePath, int type, string topic, string language, int size, int limit)
		{
			if (type < 0 && type > 2) throw new Exception("invalid type");
			var joinedPath = string.Format(filePath,
				type == 0 ? "_comments" : type == 1 ? "_captions" : type == 2 ? "_bios" : null);

			var reader = File.ReadAllLines(joinedPath).Skip(1).Select(_=>_.Split(",")).
				Where(l=>l.Count(p=>!string.IsNullOrEmpty(p))==3).
				Select(v=>new { Text = v[0].Replace("\"", ""), Topic = v[1].Replace("\"", ""), Language = v[2].Replace("\"", "") })
				.Where(x=>!x.Text.Contains("@"));

			if(!reader.Any()) return null;
			var s = Regex.Replace(string.Join(',', reader.Select(sa => sa.Text)), @"\s+", " ").TrimEnd(' ');
			var t = MarkovHelper.BuildTDict(s, size);
			return MarkovHelper.BuildString(t, limit, true).TrimEnd(' ');
		}
		#endregion



	}
}
