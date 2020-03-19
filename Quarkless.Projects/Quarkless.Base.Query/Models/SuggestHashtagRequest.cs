using System.Collections.Generic;
using Quarkless.Models.Common.Models.Topic;

namespace Quarkless.Base.Query.Models
{
	public class SuggestHashtagRequest
	{
		public Quarkless.Models.Common.Models.Topic.Topic ProfileTopic { get; set; }
		public CTopic MediaTopic { get; set; }
		public int PickAmount { get; set; } = 20;
		public IEnumerable<string> MediaUrls { get; set; }
	}
}