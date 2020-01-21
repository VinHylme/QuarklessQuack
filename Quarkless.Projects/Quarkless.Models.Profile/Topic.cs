using Quarkless.Models.Topic;
using System.Collections.Generic;

namespace Quarkless.Models.Profile
{
	public class Topic
	{
		public CTopic Category { get; set; }
		public List<CTopic> Topics { get; set; }
	}
}