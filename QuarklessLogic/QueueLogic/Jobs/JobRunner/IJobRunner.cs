﻿using Hangfire.Storage.Monitoring;
using QuarklessLogic.QueueLogic.Jobs.JobOptions;
using QuarklessLogic.QueueLogic.Jobs.JobTypes;
using System;

namespace QuarklessLogic.QueueLogic.Jobs.JobRunner
{
	public interface IJobRunner
	{
		string Queue<TJob,TJobOptions>(Action<TJobOptions> configureJob)
			where TJobOptions : IJobOptions
			where TJob : IJob<TJobOptions>; 
		void ExecuteNow(string jobId);
		JobDetailsDto GetJobDetails(string jobId);
		JobList<ScheduledJobDto> GetScheduledJobs(int from, int limit);
		JobList<ProcessingJobDto> GetCurrentlyRunningJobs(int from, int limit);
		JobList<SucceededJobDto> GetFinishedJobs(int from, int limit);
		JobList<DeletedJobDto> GetDeletedJobs(int from, int limit);
		JobList<FailedJobDto> GetFailedJobs(int from, int limit);
		StatisticsDto GetStatistics();
		bool IsAnyJobsCurrentlyRunning(); 
		bool DeleteJob(string jobId);
	}
}