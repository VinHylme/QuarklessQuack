using Hangfire;
using Quarkless.Queue.Interfaces.Jobs;
using QuarklessContexts.Models;
using QuarklessContexts.Models.Timeline;
using System;
using System.Collections.Generic;
using System.Text;

namespace Quarkless.Queue.Services
{
	public class JobRunner : IJobRunner
	{
		private readonly IBackgroundJobClient _backgroundJobClient;
		public JobRunner(IBackgroundJobClient backgroundJobClient)
		{ 
			_backgroundJobClient = backgroundJobClient;
		}

		public void Queue<TJob, TJobOptions>(Action<TJobOptions> configureJob)
			where TJob : IJob<TJobOptions>
			where TJobOptions : IJobOptions
		{
			var jobOptions = Activator.CreateInstance<TJobOptions>();
			configureJob(jobOptions);
			var jobId = _backgroundJobClient.Enqueue<TJob>(job => job.Perform(jobOptions));

		}

	}
}
