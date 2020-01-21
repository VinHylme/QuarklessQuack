using System;
using Quarkless.Models.Timeline.Interfaces.TaskScheduler;

namespace Quarkless.Models.Timeline.TaskScheduler
{
	public class ActionTaskOptions : IJobOptions
	{
		public DateTimeOffset ExecutionTime { get; set ; }
		public Delegate ActionExecute { get; set; }
		public object[] Parameters { get; set; }
	}
}
