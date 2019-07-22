using System;
using System.Collections.Generic;
using System.Text;

namespace QuarklessContexts.JobClass
{
	public interface IJobOptions
	{
		DateTimeOffset ExecutionTime { get; set; }
	}
}
