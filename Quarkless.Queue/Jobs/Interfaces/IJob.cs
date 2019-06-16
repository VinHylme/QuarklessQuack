using RestSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Quarkless.Queue.Interfaces.Jobs
{
	public interface IJob<in TJobOptions> where TJobOptions: IJobOptions
	{
		void Perform(TJobOptions jobOptions);
	}
}
