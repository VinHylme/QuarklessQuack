using System;
using System.Linq;
using Hangfire;
using Hangfire.Storage.Monitoring;
using Quarkless.Models.Timeline.Interfaces.TaskScheduler;
using Quarkless.Models.Timeline.TaskScheduler;

namespace Quarkless.Logic.Timeline.TaskScheduler
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
			var actionDetails = (EventActionOptions)jobDetails.Job?.Args?.FirstOrDefault();

			if (actionDetails == null) return;

			actionDetails.ExecutionTime = DateTime.UtcNow.AddSeconds(10);
			_backgroundJobClient.Enqueue<EventActionJob>(job => job.Perform(actionDetails));
			DeleteJob(jobId);
		}

		public JobDetailsDto GetJobDetails (string jobId)
			=> JobStorage.Current.GetMonitoringApi().JobDetails(jobId);
		
		public StatisticsDto GetStatistics()
		{
			var exp = JobStorage.Current.JobExpirationTimeout;
			var des = JobStorage.Current.GetMonitoringApi().Queues();
			return JobStorage.Current.GetMonitoringApi().GetStatistics();
		}

		public bool DeleteJob(string jobId)
			=> _backgroundJobClient.Delete(jobId);

		public JobList<ScheduledJobDto> GetScheduledJobs(int from, int limit)
			=> JobStorage.Current.GetMonitoringApi().ScheduledJobs(from,limit);
		
		public JobList<DeletedJobDto> GetDeletedJobs(int from, int limit)
			=> JobStorage.Current.GetMonitoringApi().DeletedJobs(from, limit);
		
		public JobList<FailedJobDto> GetFailedJobs(int from, int limit)
			=> JobStorage.Current.GetMonitoringApi().FailedJobs(from, limit);

		public JobList<ProcessingJobDto> GetCurrentlyRunningJobs(int from, int limit)
			=> JobStorage.Current.GetMonitoringApi().ProcessingJobs(from,limit);
		
		public JobList<SucceededJobDto> GetFinishedJobs(int from, int limit)
			=> JobStorage.Current.GetMonitoringApi().SucceededJobs(from,limit);

		public bool IsAnyJobsCurrentlyRunning()
			=> JobStorage.Current.GetMonitoringApi().ProcessingCount() > 0;
	}
}
