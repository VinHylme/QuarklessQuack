using Quarkless.Models.Common.Models.Topic;
using System.Collections.Generic;

namespace Quarkless.Base.HashtagGenerator.Models
{
	public struct Source
	{
		public IEnumerable<byte[]> ImageBytes;
		public IEnumerable<string> ImageUrls;
		public Quarkless.Models.Common.Models.Topic.Topic ProfileTopic;
		public CTopic MediaTopic;
	}
}