using System;

namespace Quarkless.Base.ApiLogger.Models
{
	[Serializable]
	public class MaxConcurrentRequests
	{
		public bool Enabled { get; set; }
		public int Limit { get; set; }
		public int MaxQueueLength { get; set; }
		public int MaxTimeInQueue { get; set; }
		public string LimitExceededPolicy { get; set; }
		public string[] ExcludePaths { get; set; }
	}
}