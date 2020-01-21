using System.Collections.Generic;
using Quarkless.Models.Topic;

namespace Quarkless.Models.Profile
{
	public class ProfileTopicAddRequest
	{
		public string ProfileId { get; set; }
		public IEnumerable<CTopic> Topics { get; set; }
	}
}