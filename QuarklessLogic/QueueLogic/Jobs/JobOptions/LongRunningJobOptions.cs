using QuarklessContexts.JobClass;
using QuarklessContexts.Models.Timeline;
using System;

namespace QuarklessLogic.QueueLogic.Jobs.JobOptions
{
	public class LongRunningJobOptions : IJobOptions
	{
		public string ActionName { get; set; }
		public RestModel Rest { get; set; }
		public DateTimeOffset ExecutionTime { get; set; }
	}
}
