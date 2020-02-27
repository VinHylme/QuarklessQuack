using Quarkless.Models.Common.Extensions;
using Quarkless.Models.ContentInfo.Interfaces;
using Quarkless.Models.Media;
using Quarkless.Models.Profile;
using Quarkless.Models.TextGenerator.Enums;
using Quarkless.Models.Topic;
using Quarkless.Models.Utilities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Quarkless.Models.Common.Models;
using Quarkless.Models.HashtagGenerator;

namespace Quarkless.Logic.ContentInfo
{
	public class ContentInfoBuilder : IContentInfoBuilder
	{
		private readonly IUtilProviders _utilProviders;
		private readonly EmojiType[] _selectFrom = {
			EmojiType.Positive,
			EmojiType.Smileys,
			EmojiType.Symbols,
			EmojiType.AnimalsNature
		};

		public ContentInfoBuilder(IUtilProviders utilProviders)
			=> _utilProviders = utilProviders;

		public string GenerateEmoji(EmojiType emojiType = EmojiType.Positive)
			=> _utilProviders.TextGenerator.GenerateSingleEmoji(emojiType);

		public string GenerateComment(CTopic mediaTopic)
			=> _utilProviders.TextGenerator.GenerateNRandomEmojies(_selectFrom.TakeAny(1).First(),
				SecureRandom.Next(2, 4)); //await _utilProviders.TextGenerator.GenerateCommentByMarkovChain(mediaTopic, SecureRandom.Next(1, 2));

		public async Task<List<HashtagResponse>> SuggestHashtags(Source source, bool deepDive = false,
			bool includeMediaExamples = true)
		{
			var results = await _utilProviders.HashtagGenerator.SuggestHashtags(source, deepDive, includeMediaExamples);
			return results.Results;
		}

		private string CreateCaption(string defaultCaption, bool generateCaption)
		{
			var caption = string.Empty;

			if (!string.IsNullOrEmpty(defaultCaption) || !string.IsNullOrWhiteSpace(defaultCaption))
			{
				caption += defaultCaption;
				caption += Environment.NewLine;
			}

			if (!generateCaption) return caption;

			caption += _utilProviders.TextGenerator.GenerateNRandomEmojies(_selectFrom.TakeAny(1).First(),
				SecureRandom.Next(2, 5));

			caption = caption.RemoveHashtagsFromText();

			return caption;
		}

		public async Task<MediaInfo> GenerateMediaInfo(Source source, string credit = null,
			bool includeMediaExamples = true, bool deepDive = false, int hashtagPickAmount = 20,
			string defaultCaption = null, bool generateCaption = false)
		{
			var hashtagsToUse = await SuggestHashtags(source, deepDive, includeMediaExamples);

			if (!hashtagsToUse.Any())
				throw new Exception("Failed to find hashtags for image");

			return new MediaInfo
			{
				Caption = CreateCaption(defaultCaption, generateCaption),
				Credit = credit,
				Hashtags = hashtagsToUse.Select(_=>_.Name).Take(hashtagPickAmount).ToList()
			};
		}
	}
}
