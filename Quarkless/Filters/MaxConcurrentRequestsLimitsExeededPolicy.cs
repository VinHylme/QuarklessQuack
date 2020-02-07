using System.ComponentModel;

namespace Quarkless.Filters
{
	public enum MaxConcurrentRequestsLimitExceededPolicy
	{
		[Description("Drop")]
		Drop,
		[Description("FifoQueueDropTail")]
		FifoQueueDropTail,
		[Description("FifoQueueDropHead")]
		FifoQueueDropHead
	}

	public class MaxConcurrentRequestsOptions
	{
		public const int ConcurrentRequestsUnlimited = -1;
		public const int MaxTimeInQueueUnlimited = -1;

		private int _limit;
		private int _maxQueueLength;
		private int _maxTimeInQueue;

		public MaxConcurrentRequestsOptions()
		{
			_limit = ConcurrentRequestsUnlimited;
			LimitExceededPolicy = MaxConcurrentRequestsLimitExceededPolicy.Drop;
			_maxQueueLength = 0;
			_maxTimeInQueue = MaxTimeInQueueUnlimited;
		}

		public bool Enabled { get; set; } = true;

		public string[] ExcludePaths { get; set; }

		public int Limit
		{
			get => _limit;
			set => _limit = (value < ConcurrentRequestsUnlimited) ? ConcurrentRequestsUnlimited : value;
		}

		public MaxConcurrentRequestsLimitExceededPolicy LimitExceededPolicy { get; set; }

		public int MaxQueueLength
		{
			get => _maxQueueLength;
			set => _maxQueueLength = (value < 0) ? 0 : value;
		}

		public int MaxTimeInQueue
		{
			get => _maxTimeInQueue;
			set => _maxTimeInQueue = (value <= 0) ? MaxTimeInQueueUnlimited : value;
		}
	}
}
