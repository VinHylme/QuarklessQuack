using System;
using System.Collections.Generic;
using System.Text;

namespace Quarkless.Queue.Interfaces.Jobs
{
	public interface IJobOptions
	{
		DateTimeOffset ExecutionTime { get; set; }
	}
}
