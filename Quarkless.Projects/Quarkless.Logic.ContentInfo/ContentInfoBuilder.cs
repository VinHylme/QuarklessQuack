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

namespace Quarkless.Logic.ContentInfo
{
	public class ContentInfoBuilder : IContentInfoBuilder
	{
		private readonly IUtilProviders _utilProviders;
		private readonly EmojiType[] _selectFrom = new[]
		{
			EmojiType.Smileys,
			EmojiType.Symbols,
			EmojiType.AnimalsNature
		};
		public ContentInfoBuilder(IUtilProviders utilProviders)
			=> _utilProviders = utilProviders;

		public string GenerateEmoji(EmojiType emojiType = EmojiType.Positive)
			=> _utilProviders.TextGenerator.GenerateNRandomEmojies(emojiType, 3);

		public string GenerateComment(CTopic mediaTopic)
			=> _utilProviders.TextGenerator.GenerateNRandomEmojies(_selectFrom.TakeAny(1).First(),
				SecureRandom.Next(2, 4)); //await _utilProviders.TextGenerator.GenerateCommentByMarkovChain(mediaTopic, SecureRandom.Next(1, 2));


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

		public async Task<MediaInfo> GenerateMediaInfo(Topic profileTopic, CTopic mediaTopic,
			string credit = null, int hashtagPickAmount = 20, IEnumerable<string> medias = null,
			string defaultCaption = null, bool generateCaption = false)
		{
			var hashtagsToUse = await _utilProviders.HashtagGenerator.SuggestHashtags(profileTopic, mediaTopic,
				pickAmount: hashtagPickAmount, images: medias);

			if (!hashtagsToUse.Any())
				throw new Exception("Failed to find hashtags for image");

			return new MediaInfo
			{
				Caption = CreateCaption(defaultCaption, generateCaption),
				Credit = credit,
				Hashtags = hashtagsToUse
			};
		}

		public async Task<MediaInfo> GenerateMediaInfoBytes(Topic profileTopic, CTopic mediaTopic,
			string credit = null, int hashtagPickAmount = 20, IEnumerable<byte[]> medias = null,
			string defaultCaption = null, bool generateCaption = false)
		{
			var hashtagsToUse = await _utilProviders.HashtagGenerator.SuggestHashtags(profileTopic, mediaTopic,
				pickAmount: hashtagPickAmount, images: medias);

			if (!hashtagsToUse.Any())
				throw new Exception("Failed to find hashtags for image");

			return new MediaInfo
			{
				Caption = CreateCaption(defaultCaption, generateCaption),
				Credit = credit,
				Hashtags = hashtagsToUse
			};
		}
	}
}
