using System.Collections.Generic;
using Quarkless.Models.Topic;

namespace Quarkless.Models.Media
{
	public class From
	{
		public int TopicHash { get; set; }
		public CTopic CategoryTopic { get; set; }
		public CTopic TopicRequest { get; set; }
		public List<CTopic> TopicTree { get; set; }
		public string InstagramAccountId { get; set; }
		public string AccountId { get; set; }
	}
}