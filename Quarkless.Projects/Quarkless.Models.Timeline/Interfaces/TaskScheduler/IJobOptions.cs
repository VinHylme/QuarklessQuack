using System;

namespace Quarkless.Models.Timeline.Interfaces.TaskScheduler
{
	public interface IJobOptions
	{
		DateTimeOffset ExecutionTime { get; set; }
	}
}
