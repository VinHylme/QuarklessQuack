using System;

namespace QuarklessLogic.QueueLogic.Jobs.JobOptions
{
	public class ActionTaskOptions : IJobOptions
	{
		public DateTimeOffset ExecutionTime { get; set ; }
		public Delegate ActionExecute { get; set; }
		public object[] Parameters { get; set; }
	}
}
