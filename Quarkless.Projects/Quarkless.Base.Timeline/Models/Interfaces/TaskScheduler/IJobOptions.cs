using System;

namespace Quarkless.Base.Timeline.Models.Interfaces.TaskScheduler
{
	public interface IJobOptions
	{
		DateTimeOffset ExecutionTime { get; set; }
	}
}
