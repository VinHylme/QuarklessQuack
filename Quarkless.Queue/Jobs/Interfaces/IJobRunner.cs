using QuarklessContexts.Models;
using QuarklessContexts.Models.Timeline;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Quarkless.Queue.Interfaces.Jobs
{
	public interface IJobRunner
	{
		void Queue<TJob,TJobOptions>(Action<TJobOptions> configureJob)
			where TJobOptions : IJobOptions
			where TJob : IJob<TJobOptions>;
	}
}
