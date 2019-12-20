using System.Collections.Generic;
using QuarklessContexts.Models.Topics;

namespace QuarklessContexts.Models.Profiles
{
	public class Topic
	{
		public CTopic Category { get; set; }
		public List<CTopic> Topics { get; set; }
	}
}