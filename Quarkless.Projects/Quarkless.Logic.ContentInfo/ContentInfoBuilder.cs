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

		public string GenerateComment(CTopic mediaTopic)
			=> _utilProviders.TextGenerator.GenerateNRandomEmojies(_selectFrom.TakeAny(1).First(),
				SecureRandom.Next(2, 4)); //await _utilProviders.TextGenerator.GenerateCommentByMarkovChain(mediaTopic, SecureRandom.Next(1, 2));

		public async Task<MediaInfo> GenerateMediaInfo(Topic profileTopic, CTopic mediaTopic,
			string credit = null, int hashtagPickAmount = 20, IEnumerable<string> medias = null)
		{
			var hashtagsToUse = await _utilProviders.HashtagGenerator.SuggestHashtags(profileTopic, mediaTopic,
				pickAmount: hashtagPickAmount, images: medias);
			if (!hashtagsToUse.Any())
			{
				throw new Exception("Failed to find hashtags for image");
			}

			var caption = _utilProviders.TextGenerator.GenerateNRandomEmojies(_selectFrom.TakeAny(1).First(),
					SecureRandom.Next(2, 5));
			//.GenerateCaptionByMarkovChain(mediaTopic,SecureRandom.Next(1, 2));

			return new MediaInfo
			{
				Caption = caption,
				Credit = credit,
				Hashtags = hashtagsToUse
			};
		}
		public async Task<MediaInfo> GenerateMediaInfoBytes(Topic profileTopic, CTopic mediaTopic,
			string credit = null, int hashtagPickAmount = 20, IEnumerable<byte[]> medias = null)
		{
			var hashtagsToUse = await _utilProviders.HashtagGenerator.SuggestHashtags(profileTopic, mediaTopic,
				pickAmount: hashtagPickAmount, images: medias);
			if (!hashtagsToUse.Any())
			{
				throw new Exception("Failed to find hashtags for image");
			}

			var caption = _utilProviders.TextGenerator.GenerateNRandomEmojies(_selectFrom.TakeAny(1).First(),
				SecureRandom.Next(2, 5));
			//.GenerateCaptionByMarkovChain(mediaTopic,SecureRandom.Next(1, 2));

			return new MediaInfo
			{
				Caption = caption,
				Credit = credit,
				Hashtags = hashtagsToUse
			};
		}
	}
}
