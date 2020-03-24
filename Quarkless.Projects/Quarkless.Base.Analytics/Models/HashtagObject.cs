using Quarkless.Base.Analytics.Models.Enums;

namespace Quarkless.Base.Analytics.Models
{
	public class HashtagObject
	{
		public string Name { get; set; }
		public bool IsBanned { get; set; }
		public HashtagRarity Rarity { get; set; }
		public long MediaCount { get; set; }
		public double SimilarityDistanceFromParent { get; set; }
	}
}