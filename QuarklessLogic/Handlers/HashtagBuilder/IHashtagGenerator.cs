using System.Collections.Generic;
using System.Threading.Tasks;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.Topics;

namespace QuarklessLogic.Handlers.HashtagBuilder
{
	public interface IHashtagGenerator
	{
		Task<List<string>> SuggestHashtags(Topic profileTopic, CTopic mediaTopic,
			int pickAmount = 20, IEnumerable<string> imageUrls = null);
		//Task<IEnumerable<string>> BuildHashtags(string topic, string subcategory, string language = null, int limit = 1, int pickRate = 20);
	}
}