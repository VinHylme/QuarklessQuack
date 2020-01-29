using Quarkless.Models.Media;
using Quarkless.Models.Topic;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Quarkless.Models.ContentInfo.Interfaces
{
	public interface IContentInfoBuilder
	{
		string GenerateComment(CTopic mediaTopic);

		Task<MediaInfo> GenerateMediaInfo(Profile.Topic profileTopic, CTopic mediaTopic,
			string credit = null, int hashtagPickAmount = 20, IEnumerable<string> medias = null);

		Task<MediaInfo> GenerateMediaInfoBytes(Profile.Topic profileTopic, CTopic mediaTopic,
			string credit = null, int hashtagPickAmount = 20, IEnumerable<byte[]> medias = null);
	}
}