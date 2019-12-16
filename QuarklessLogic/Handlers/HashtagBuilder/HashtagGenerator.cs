using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MoreLinq;
using Quarkless.Vision;
using QuarklessContexts.Extensions;
using QuarklessLogic.Logic.HashtagLogic;

namespace QuarklessLogic.Handlers.HashtagBuilder
{
	public class HashtagGenerator
	{
		private readonly IHashtagLogic _hashtagLogic;
		private readonly IVisionClient _visionClient;
		public HashtagGenerator(IHashtagLogic hashtagLogic, IVisionClient client)
		{
			_hashtagLogic = hashtagLogic;
			_visionClient = client;
		}

		public async Task<IEnumerable<string>> SuggestHashtags(byte[] imageBytes, string topic)
		{
			var imageLabels = await _visionClient.AnnotateImage(imageBytes);
			return null;
		}
		public async Task<IEnumerable<string>> BuildHashtags(string topic, string subcategory, string language = null, int limit = 1, int pickRate = 20)
		{
			var hashtagResults = (await _hashtagLogic.GetHashtagsByTopicAndLanguage(topic.OnlyWords(),
				language?.ToUpper().OnlyWords(), 
				language?.MapLanguages().OnlyWords(), limit))
				.Shuffle().ToList();
			
			var clean = new Regex(@"[^\w\d]");
			if (hashtagResults.Count <= 0) return null;
			var hashtags = new List<string>();
			while (hashtags.Count < pickRate)
			{
				var chosenHashtags = new List<string>();

				foreach (var hashtagResult in hashtagResults)
				{
					if (string.IsNullOrEmpty(hashtagResult.Language)) continue;
					var hashtagLanguage = clean.Replace(hashtagResult.Language.ToLower(), "");
					var languageSelected = clean.Replace(language.MapLanguages().ToLower(), "");

					if (hashtagLanguage == languageSelected)
						chosenHashtags.AddRange(hashtagResult.Hashtags);
				}

				if (chosenHashtags.Count <= 0) continue;
				var chosenHashtagsFiltered = chosenHashtags.Where(space => space.Count(oc => oc == ' ') <= 1);
				var hashtagsFiltered = chosenHashtagsFiltered as string[] ?? chosenHashtagsFiltered.ToArray();
				if (!hashtagsFiltered.Any()) return null;
				hashtags.AddRange(hashtagsFiltered.Where(s => s.Length >= 3 && s.Length <= 30));
			}
			return hashtags;
		}

	}
}
