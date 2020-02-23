using System.Collections.Generic;
using Quarkless.Models.Topic;

namespace Quarkless.Models.HashtagGenerator
{
	public struct Source
	{
		public IEnumerable<byte[]> ImageBytes;
		public IEnumerable<string> ImageUrls;
		public Profile.Topic ProfileTopic;
		public CTopic MediaTopic;
	}
}