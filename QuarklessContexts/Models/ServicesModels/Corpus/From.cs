using System.Collections.Generic;
using QuarklessContexts.Models.Topics;

namespace QuarklessContexts.Models.ServicesModels.Corpus
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