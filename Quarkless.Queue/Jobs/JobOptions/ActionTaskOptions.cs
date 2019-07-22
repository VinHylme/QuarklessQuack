using QuarklessContexts.JobClass;
using System;
using System.Collections.Generic;
using System.Text;

namespace Quarkless.Queue.Jobs.JobOptions
{
	public class ActionTaskOptions : IJobOptions
	{
		public DateTimeOffset ExecutionTime { get; set ; }
		public Delegate ActionExecute { get; set; }
		public object[] Parameters { get; set; }
	}
}
