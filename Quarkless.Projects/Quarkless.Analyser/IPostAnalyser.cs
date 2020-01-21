namespace Quarkless.Analyser
{
	public interface IPostAnalyser
	{
		IFilters Filters { get; }
		IMediaManager Manager { get; }
		IMediaManipulation Manipulation { get; }
	}
}