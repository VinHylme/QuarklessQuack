using System.Collections.Generic;

namespace Quarkless.Models.Common.Models.Topic
{
	public class Topic
	{
		public CTopic Category { get; set; }
		public List<CTopic> Topics { get; set; }
	}
}