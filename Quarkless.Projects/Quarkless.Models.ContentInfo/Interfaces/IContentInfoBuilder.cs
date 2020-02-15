using Quarkless.Models.Media;
using Quarkless.Models.TextGenerator.Enums;
using Quarkless.Models.Topic;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Quarkless.Models.ContentInfo.Interfaces
{
	public interface IContentInfoBuilder
	{
		string GenerateEmoji(EmojiType emojiType = EmojiType.Positive);
		string GenerateComment(CTopic mediaTopic);

		Task<MediaInfo> GenerateMediaInfo(Profile.Topic profileTopic, CTopic mediaTopic,
			string credit = null, int hashtagPickAmount = 20,
			IEnumerable<string> medias = null,string defaultCaption = null, bool generateCaption = false);

		Task<MediaInfo> GenerateMediaInfoBytes(Profile.Topic profileTopic, CTopic mediaTopic,
			string credit = null, int hashtagPickAmount = 20,
			IEnumerable<byte[]> medias = null, string defaultCaption = null, bool generateCaption = false);
	}
}