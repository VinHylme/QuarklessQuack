using System.Collections.Generic;

namespace Quarkless.Base.ContentSearch.Models.Models
{
	public class SubTopics
	{
		public string TopicName { get; set; }
		public List<string> RelatedTopics { get; set; }
	}
}