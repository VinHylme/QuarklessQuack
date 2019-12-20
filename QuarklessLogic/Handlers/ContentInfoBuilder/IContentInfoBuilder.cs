using System.Collections.Generic;
using System.Threading.Tasks;
using QuarklessContexts.Models.MediaModels;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.Topics;

namespace QuarklessLogic.Handlers.ContentInfoBuilder
{
	public interface IContentInfoBuilder
	{
		Task<string> GenerateComment(CTopic mediaTopic);

		Task<MediaInfo> GenerateMediaInfo(Topic profileTopic, CTopic mediaTopic, 
			string credit = null, int hashtagPickAmount = 20, IEnumerable<string> medias = null);
	}
}