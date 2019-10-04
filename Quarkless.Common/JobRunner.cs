﻿using Hangfire;
using Hangfire.States;
using Hangfire.Storage.Monitoring;
using QuarklessContexts.JobClass;
using QuarklessLogic.QueueLogic.Jobs.JobOptions;
using QuarklessLogic.QueueLogic.Jobs.JobTypes;
using System;
using System.Linq;

namespace Quarkless.Common
{
	public class JobRunner : IJobRunner
	{
		private readonly IBackgroundJobClient _backgroundJobClient;
		public JobRunner(IBackgroundJobClient backgroundJobClient)
		{ 
			_backgroundJobClient = backgroundJobClient;
		}
		public string Queue<TJob, TJobOptions>(Action<TJobOptions> configureJob)
			where TJob : IJob<TJobOptions>
			where TJobOptions : IJobOptions
		{
			var jobOptions = Activator.CreateInstance<TJobOptions>();
			configureJob(jobOptions);
			var jobId = _backgroundJobClient.Schedule<TJob>(job => job.Perform(jobOptions), jobOptions.ExecutionTime);
			return jobId;
		}
		public void ExecuteNow(string jobId)
		{
			var jobDetails = JobStorage.Current.GetMonitoringApi().JobDetails(jobId);
			var actionDetails = ((LongRunningJobOptions)jobDetails.Job?.Args?.FirstOrDefault());
			if (actionDetails != null)
			{
				actionDetails.ExecutionTime = DateTime.UtcNow.AddSeconds(10);
				_backgroundJobClient.Enqueue<LongRunningJob>(job => job.Perform(actionDetails));
			}

			DeleteJob(jobId);
		}
		public JobDetailsDto GetJobDetails (string jobId)
		{
			return JobStorage.Current.GetMonitoringApi().JobDetails(jobId);
		}
		public StatisticsDto GetStatistics()
		{
			var exp = JobStorage.Current.JobExpirationTimeout;
			var des = JobStorage.Current.GetMonitoringApi().Queues();
			return JobStorage.Current.GetMonitoringApi().GetStatistics();
		}
		public bool DeleteJob(string jobId)
		{
			return _backgroundJobClient.Delete(jobId);
		}
		public JobList<ScheduledJobDto> GetScheduledJobs(int from, int limit)
		{
			return JobStorage.Current.GetMonitoringApi().ScheduledJobs(from,limit);
		}
		public JobList<DeletedJobDto> GetDeletedJobs(int from, int limit)
		{
			return JobStorage.Current.GetMonitoringApi().DeletedJobs(from, limit);
		}
		public JobList<FailedJobDto> GetFailedJobs(int from, int limit)
		{
			return JobStorage.Current.GetMonitoringApi().FailedJobs(from, limit);
		}
		public JobList<ProcessingJobDto> GetCurrentlyRunningJobs(int from, int limit)
		{
			return JobStorage.Current.GetMonitoringApi().ProcessingJobs(from,limit);
		}
		public JobList<SucceededJobDto> GetFinishedJobs(int from, int limit) { 
			return JobStorage.Current.GetMonitoringApi().SucceededJobs(from,limit);
		}
		public bool IsAnyJobsCurrentlyRunning()
		{
			return JobStorage.Current.GetMonitoringApi().ProcessingCount()>0;
		}
	}
}
