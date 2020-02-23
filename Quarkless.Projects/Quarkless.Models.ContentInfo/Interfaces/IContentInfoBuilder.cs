using Quarkless.Models.Media;
using Quarkless.Models.TextGenerator.Enums;
using Quarkless.Models.Topic;
using System.Collections.Generic;
using System.Threading.Tasks;
using Quarkless.Models.Common.Models;
using Quarkless.Models.HashtagGenerator;

namespace Quarkless.Models.ContentInfo.Interfaces
{
	public interface IContentInfoBuilder
	{
		string GenerateEmoji(EmojiType emojiType = EmojiType.Positive);
		string GenerateComment(CTopic mediaTopic);

		Task<List<HashtagResponse>> SuggestHashtags(Source source, bool deepDive = false, bool includeMediaExamples = true);

		Task<MediaInfo> GenerateMediaInfo(Source source, string credit = null,
			bool includeMediaExamples = true, bool deepDive = false, int hashtagPickAmount = 20,
			string defaultCaption = null, bool generateCaption = false);
	}
}