using Quarkless.Models.Common.Enums;

namespace Quarkless.Models.Common.Models
{
	public class HashtagResponse
	{
		public string Name { get; set; }
		public HashtagRarity Rarity { get; set; }
		public bool IsRecommended { get; set; }
		public bool IsMediaExample { get; set; }
		public bool IsMediaDescriptor { get; set; }
	}
}