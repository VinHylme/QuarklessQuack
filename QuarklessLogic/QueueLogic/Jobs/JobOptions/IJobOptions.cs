using System;

namespace QuarklessLogic.QueueLogic.Jobs.JobOptions
{
	public interface IJobOptions
	{
		DateTimeOffset ExecutionTime { get; set; }
	}
}
