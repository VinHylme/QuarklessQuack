using System;
using Quarkless.Models.Timeline.Interfaces.TaskScheduler;

namespace Quarkless.Models.Timeline.TaskScheduler
{
	public class LongRunningJobOptions : IJobOptions
	{
		public string ActionName { get; set; }
		public RestModel Rest { get; set; }
		public DateTimeOffset ExecutionTime { get; set; }
	}
}
