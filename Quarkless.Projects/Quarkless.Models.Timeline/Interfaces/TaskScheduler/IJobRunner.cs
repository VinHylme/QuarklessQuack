using System;
using Hangfire.Storage.Monitoring;

namespace Quarkless.Models.Timeline.Interfaces.TaskScheduler
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
