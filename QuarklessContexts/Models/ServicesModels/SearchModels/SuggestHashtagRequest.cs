using System.Collections.Generic;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.Topics;

namespace QuarklessContexts.Models.ServicesModels.SearchModels
{
	public class SuggestHashtagRequest
	{
		public Topic ProfileTopic { get; set; }
		public CTopic MediaTopic { get; set; }
		public int PickAmount { get; set; } = 20;
		public IEnumerable<string> MediaUrls { get; set; }
	}
}