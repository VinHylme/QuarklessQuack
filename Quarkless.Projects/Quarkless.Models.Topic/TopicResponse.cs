using System.Collections.Generic;

namespace Quarkless.Models.Topic
{
	public class TopicResponse
	{
		public CTopic Topic { get; set; }
		public IEnumerable<string> RelatedTopicsFound { get; set; }
	}
}