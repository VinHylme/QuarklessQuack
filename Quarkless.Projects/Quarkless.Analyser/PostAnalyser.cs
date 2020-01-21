namespace Quarkless.Analyser
{
	public class PostAnalyser : IPostAnalyser
	{
		public IFilters Filters => new Filters();
		public IMediaManipulation Manipulation { get; }
		public IMediaManager Manager => new MediaManager();
		public PostAnalyser(IMediaManipulation mediaManipulation)
		{
			Manipulation = mediaManipulation;
		}
	}
}
