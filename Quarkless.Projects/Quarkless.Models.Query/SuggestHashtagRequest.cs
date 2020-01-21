using System.Collections.Generic;
using Quarkless.Models.Topic;

namespace Quarkless.Models.Query
{
	public class SuggestHashtagRequest
	{
		public Profile.Topic ProfileTopic { get; set; }
		public CTopic MediaTopic { get; set; }
		public int PickAmount { get; set; } = 20;
		public IEnumerable<string> MediaUrls { get; set; }
	}
}