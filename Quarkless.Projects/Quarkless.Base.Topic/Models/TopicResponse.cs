using Quarkless.Models.Common.Models.Topic;
using System.Collections.Generic;

namespace Quarkless.Base.Topic.Models
{
	public class TopicResponse
	{
		public CTopic Topic { get; set; }
		public IEnumerable<string> RelatedTopicsFound { get; set; }
	}
}