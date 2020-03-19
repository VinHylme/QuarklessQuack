using Quarkless.Models.Common.Objects;

namespace Quarkless.Base.HashtagGenerator.Models
{
	public struct KeyWordsContainer
	{
		public SString Keyword { get; set; }
		public bool IsLikely { get; set; }
		public int TypeId { get; set; }
		public KeyWordsContainer(SString keyword, bool isLikely, int typeId)
		{
			Keyword = keyword;
			IsLikely = isLikely;
			TypeId = typeId;
		}
	}
}
