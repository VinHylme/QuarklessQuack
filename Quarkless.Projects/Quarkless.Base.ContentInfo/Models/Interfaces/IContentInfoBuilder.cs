using System.Collections.Generic;
using System.Threading.Tasks;
using Quarkless.Base.HashtagGenerator.Models;
using Quarkless.Base.Media.Models;
using Quarkless.Base.TextGenerator.Models.Enums;
using Quarkless.Models.Common.Models;
using Quarkless.Models.Common.Models.Topic;

namespace Quarkless.Base.ContentInfo.Models.Interfaces
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