using System;
using System.IO;
using System.Text.RegularExpressions;
using WDict = System.Collections.Generic.Dictionary<string, uint>;
using TDict = System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, uint>>;
using QuarklessContexts.Extensions;
using System.Text;
using System.Linq;
using QuarklessRepositories.Repository.CorpusRepositories.Comments;
using QuarklessRepositories.Repository.CorpusRepositories.Medias;
using MongoDB.Driver;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Globalization;

namespace QuarklessLogic.Handlers.TextGeneration
{
	public class TextGeneration : ITextGeneration
	{
		private readonly ICommentCorpusRepository _commentCorpusRepository;
		private readonly IMediaCorpusRepository _mediaCorpusRepository;
		public TextGeneration(ICommentCorpusRepository commentCorpusRepository, IMediaCorpusRepository mediaCorpusRepository)
		{
			_commentCorpusRepository = commentCorpusRepository;
			_mediaCorpusRepository = mediaCorpusRepository;
		}
		//TODO: ADD AI LATER, FOR NOW JUST USING MARKOV ALGO
		public string MarkovTextGenerator(string filePath, int limit, int size, bool exact = false)
		{
			if (!File.Exists(filePath)) { Console.WriteLine("Input file doesn't exist"); return null; }

			string s = Regex.Replace(File.ReadAllText(filePath), @"\s+", " ").TrimEnd(' ');
			TDict t = MarkovHelper.BuildTDict(s, size);
			return MarkovHelper.BuildString(t, limit, exact).TrimEnd(' ');
		}
		public async Task<string> MarkovIt(int type, string topic, string language, int size, int limit)
		{
			if (type < 0 && type > 2) throw new Exception("invalid type");
			Regex exlude = new Regex(@"(^(http:\/\/www\.|https:\/\/www\.|http:\/\/|https:\/\/)?[a-z0-9]+([\-\.]{1}[a-z0-9]+)*\.[a-z]{2,5}(:[0-9]{1,5})?(\/.*)?$)|(@)|(\d{5})");
			
			switch (type)
			{
				case 0:
					var data = await _commentCorpusRepository.GetComments(topic,language.ToUpper(),language.MapLanguages(),5000);
					if (data != null)
					{
						string choice = Regex.Replace(string.Join(',', data.Where(s=>!s.Comment.Contains('@') || !exlude.IsMatch(s.Comment)).Select(sa => sa.Comment)), @"\s+", " ").TrimEnd(' ');
						TDict t = MarkovHelper.BuildTDict(choice, size);
						return MarkovHelper.BuildString(t, limit, true).TrimEnd(' ');
					}
					return null;
				case 1:
					var mdata = await _mediaCorpusRepository.GetMedias(topic, language.ToUpper(),language.MapLanguages(),5000);
					if (mdata != null)
					{
						string choice = Regex.Replace(string.Join(',', mdata.Where(sc=>!sc.Caption.Contains('@') || !exlude.IsMatch(sc.Caption)).Select(sa => sa.Caption)), @"\s+", " ").TrimEnd(' ');
						TDict t = MarkovHelper.BuildTDict(choice, size);
						return MarkovHelper.BuildString(t, limit, true).TrimEnd(' ');
					}
					return null;
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
