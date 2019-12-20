using Quarkless.Interfacing.Objects;

namespace QuarklessLogic.Handlers.HashtagBuilder
{
	public struct KeyWordsContainer
	{
		public SString Keyword { get; set; }
		public bool IsLikely { get; set; }
		public KeyWordsContainer(SString keyword, bool isLikely)
		{
			Keyword = keyword;
			IsLikely = isLikely;
		}
	}
}