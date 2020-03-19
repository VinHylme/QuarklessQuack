using System.Collections.Generic;
using Quarkless.Models.Common.Interfaces;
using Quarkless.Models.Common.Models.Resolver;

namespace Quarkless.Base.Stories.Models
{
	public class StoryRequest : IExec
	{
		public long UserId { get; set; }
		public string StoryId { get; set; }
		public string Reaction { get; set; }
		public bool ContainsItems { get; set; }
		public List<StoryItem> Items { get; set; }
		public MediaShort Media { get; set; }
		public UserShort User { get; set; }
		public DataFrom DataFrom { get; set; }
	}
}