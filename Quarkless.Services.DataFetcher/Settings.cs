namespace Quarkless.Services.DataFetcher
{
	public class Settings
	{
		public string AccountId { get; set; }
		public double IntervalWaitBetweenHashtagsAndMediaSearch { get; set; } = .55;
		public int MediaFetchAmount { get; set; } = 1;
		public int CommentFetchAmount { get; set; } = 1;
		public int BatchSize { get; set; } = 3;
		public int WorkerAccountType { get; set; } = 1;
		public bool BuildInitialTopics { get; set; }
	}
}