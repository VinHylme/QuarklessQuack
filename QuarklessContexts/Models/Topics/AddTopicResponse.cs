using System.Collections.Generic;

namespace QuarklessContexts.Models.Topics
{
	public class AddTopicResponse
	{
		public string Id { get; set; }
		public bool Exists { get; set; }
	}

	public class TopicResponse
	{
		public CTopic Topic { get; set; }
		public IEnumerable<string> RelatedTopicsFound { get; set; }
	}
}
