using System.Collections.Generic;
using Quarkless.Models.Common.Models.Topic;

namespace Quarkless.Events.Models
{
	public class ProfileTopicAddRequest
	{
		public string ProfileId { get; set; }
		public IEnumerable<CTopic> Topics { get; set; }
	}
}