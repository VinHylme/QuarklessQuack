using System;

namespace QuarklessContexts.JobClass
{
	public interface IJobOptions
	{
		DateTimeOffset ExecutionTime { get; set; }
	}
}
