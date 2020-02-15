using System.Collections.Generic;
using Quarkless.Models.Common.Interfaces;

namespace Quarkless.Models.Stories
{
	public class StoryRequest : IExec
	{
		public long UserId { get; set; }
		public string StoryId { get; set; }
		public string Reaction { get; set; }
		public bool ContainsItems { get; set; }
		public List<StoryItem> Items { get; set; }
	}
}