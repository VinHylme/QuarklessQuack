using System;
using System.IO;
using System.Text.RegularExpressions;
using WDict = System.Collections.Generic.Dictionary<string, uint>;
using TDict = System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, uint>>;
using QuarklessContexts.Extensions;
using System.Text;
using System.Linq;
using QuarklessRepositories.Repository.CorpusRepositories.Medias;
using MongoDB.Driver;
using System.Threading.Tasks;
using MoreLinq;
using QuarklessLogic.ServicesLogic.CorpusLogic;

namespace QuarklessLogic.Handlers.TextGeneration
{
	public class TextGeneration : ITextGeneration
	{
		private readonly ICommentCorpusLogic _commentCorpusLogic;
		private readonly IMediaCorpusLogic _mediaCorpusLogic;
		public TextGeneration(ICommentCorpusLogic commentCorpusLogic, IMediaCorpusLogic mediaCorpusLogic)
		{
			_commentCorpusLogic = commentCorpusLogic;
			_mediaCorpusLogic = mediaCorpusLogic;
		}
		//TODO: ADD AI LATER, FOR NOW JUST USING MARKOV ALGO
		public string MarkovTextGenerator(string filePath, int limit, int size, bool exact = false)
		{
			if (!File.Exists(filePath)) { Console.WriteLine("Input file doesn't exist"); return null; }

			string s = Regex.Replace(File.ReadAllText(filePath), @"\s+", " ").TrimEnd(' ');
			TDict t = MarkovHelper.BuildTDict(s, size);
			return MarkovHelper.BuildString(t, limit, exact).TrimEnd(' ');
		}
		public async Task<string> MarkovIt(int type, string topic, string language, int limit)
		{
			if (type < 0 && type > 2) throw new Exception("invalid type");
			switch (type)
			{
				case 0:
					var data = (await _commentCorpusLogic
						.GetComments(topic.OnlyWords(), language.MapLanguages().OnlyWords(),50000))
						.DistinctBy(x=>x.Comment);
					if (data == null) return null;
					data = data.Where(x => !x.Comment.ContainsHashtags()
						&& !x.Comment.ContainsMentions()
						&& !x.Comment.ContainsPhoneNumber()
						&& !x.Comment.ContainsWebAddress());
					var dataComment = Regex.Replace(string.Join(',', data.Select(sa => sa.Comment)), @"\s+", " ").TrimEnd(' ');
					var dCommentDict = MarkovHelper.BuildTDict(dataComment, SecureRandom.Next(10,60));
					return MarkovHelper.BuildString(dCommentDict, limit, true).TrimEnd(' ').Replace(","," ");
				case 1:
					var metaMedia = (await _mediaCorpusLogic
						.GetMedias(topic.OnlyWords(), language.MapLanguages().OnlyWords(), 50000))
						.DistinctBy(x => x.Caption);
					metaMedia = metaMedia.Where(x => !x.Caption.ContainsHashtags()
						&& !x.Caption.ContainsMentions()
						&& !x.Caption.ContainsPhoneNumber()
						&& !x.Caption.ContainsWebAddress());
					
					var dataMedia = Regex.Replace(string.Join(',', metaMedia.Select(sa => sa.Caption)), @"\s+", " ").TrimEnd(' ');
					var dMediaDict = MarkovHelper.BuildTDict(dataMedia, SecureRandom.Next(10,60));
					return MarkovHelper.BuildString(dMediaDict, limit, true).TrimEnd(' ').Replace(","," ");
				case 2:
					break;
			}
			return null;
		}
		public string MarkovTextGenerator(string filePath, int type, string topic, string language, int size, int limit)
		{
			if (type < 0 && type > 2) throw new Exception("invalid type");
			string joinedPath = string.Format(filePath,
				type == 0 ? "_comments" : type == 1 ? "_captions" : type == 2 ? "_bios" : null);

			var reader = File.ReadAllLines(joinedPath).Skip(1).Select(_=>_.Split(",")).
				Where(l=>l.Count(p=>!string.IsNullOrEmpty(p))==3).
				Select(v=>new { Text = v[0].Replace("\"", ""), Topic = v[1].Replace("\"", ""), Language = v[2].Replace("\"", "") })
				.Where(x=>!x.Text.Contains("@"));

			if(reader==null && reader.Count()<=0) return null;
			string s = Regex.Replace(string.Join(',', reader.Select(sa => sa.Text)), @"\s+", " ").TrimEnd(' ');
			TDict t = MarkovHelper.BuildTDict(s, size);
			return MarkovHelper.BuildString(t, limit, true).TrimEnd(' ');
		}

	}
}
