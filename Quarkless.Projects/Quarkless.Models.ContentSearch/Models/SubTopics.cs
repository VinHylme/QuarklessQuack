using System.Collections.Generic;

namespace Quarkless.Models.ContentSearch.Models
{
	public class SubTopics
	{
		public string TopicName { get; set; }
		public List<string> RelatedTopics { get; set; }
	}
}