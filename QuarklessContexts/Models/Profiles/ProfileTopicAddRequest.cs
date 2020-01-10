using System.Collections.Generic;
using QuarklessContexts.Models.Topics;

namespace QuarklessContexts.Models.Profiles
{
	public class ProfileTopicAddRequest
	{
		public string ProfileId { get; set; }
		public IEnumerable<CTopic> Topics { get; set; }
	}
}