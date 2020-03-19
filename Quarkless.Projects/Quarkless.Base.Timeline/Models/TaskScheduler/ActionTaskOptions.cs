using System;
using Quarkless.Base.Timeline.Models.Interfaces.TaskScheduler;

namespace Quarkless.Base.Timeline.Models.TaskScheduler
{
	public class ActionTaskOptions : IJobOptions
	{
		public DateTimeOffset ExecutionTime { get; set ; }
		public Delegate ActionExecute { get; set; }
		public object[] Parameters { get; set; }
	}
}
