using Quarkless.Queue.Interfaces.Jobs;
using Quarkless.Queue.Jobs.Interfaces;
using Quarkless.Queue.Jobs.JobOptions;
using Quarkless.Queue.Jobs.JobTypes;
using QuarklessContexts.Models.Timeline;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Quarkless.Queue.Services
{
	public class TaskService : ITaskService
	{
		private readonly IJobRunner _jobRunner;
		public TaskService(IJobRunner jobRunner)
		{
			_jobRunner = jobRunner;
		}

		public void ActionTask(Delegate @delegate, DateTimeOffset executeTime, params object[] args)
		{
			_jobRunner.QueueActionTaskJob(o =>
			{
				o.ActionExecute = @delegate;
				o.ExecutionTime = executeTime;
				o.Parameters = args;
			});
		}
		public string ScheduleEvent(string ActionName, RestModel restModel, DateTimeOffset timeOffset)
		{
			return _jobRunner.AddScheduledJob(op=>
			{
				op.ActionName = ActionName;
				op.Rest = restModel;
				op.ExecutionTime = timeOffset;
			});
		}
		public TimelineItemDetail GetEvent(string itemId)
		{
			var job = _jobRunner.GetJobDetails(itemId);
			var actionDetails = ((LongRunningJobOptions)job.Job.Args.FirstOrDefault());

			return new TimelineItemDetail { 
				ItemId = itemId,
				ActionName = actionDetails.ActionName,
				CreatedTime = job.CreatedAt,
				ExpireAt = job.ExpireAt,
				ExecuteTime = actionDetails.ExecutionTime,
				Url = actionDetails.Rest.BaseUrl,
				User = actionDetails.Rest.User,
				History = job.History.Select(_=> new ItemHistory
				{
					CreatedAt = _.CreatedAt,
					Reason = _.Reason,
					StateName = _.StateName,
					Data = _.Data
				}).ToList()
			};
		}
		public IEnumerable<TimelineItem> GetScheduledItemsForUser(string username, string instagramId = null, int limit = 30)
		{
			IEnumerable<TimelineItem> total;
			if (string.IsNullOrEmpty(instagramId)) 
				total = GetTotalScheduledEvents(0,int.MaxValue).Where(_=>_.User.OAccountId == username);
			else
				total = GetTotalScheduledEvents(0, int.MaxValue).Where(_ => _.User.OAccountId == username && _.User.OInstagramAccountUser == instagramId);
			
			return total?.Take(limit) ?? null;
		}
		public IEnumerable<TimelineInProgressItem> GetCurrentlyRunningItemsForUser(string username, string instagramId = null, int limit = 30)
		{
			IEnumerable<TimelineInProgressItem> total;
			if (string.IsNullOrEmpty(instagramId))
				total = GetTotalCurrentlyRunningJobs(0, int.MaxValue).Where(_ => _.User.OAccountId == username);
			else
				total = GetTotalCurrentlyRunningJobs(0, int.MaxValue).Where(_ => _.User.OAccountId == username && _.User.OInstagramAccountUser == instagramId);

			return total?.Take(limit) ?? null;
		}
		public IEnumerable<TimelineFinishedItem> GetFinishedItemsForUser(string username, string instagramId = null, int limit = 30)
		{
			IEnumerable<TimelineFinishedItem> total;
			if (string.IsNullOrEmpty(instagramId))
				total = GetTotalFinishedJobs(0, int.MaxValue).Where(_ => _.User.OAccountId == username);
			else
				total = GetTotalFinishedJobs(0, int.MaxValue).Where(_ => _.User.OAccountId == username && _.User.OInstagramAccountUser == instagramId);

			return total?.Take(limit) ?? null;
		}
		public IEnumerable<TimelineDeletedItem> GetDeletedItemsForUser(string username, string instagramId = null, int limit = 30)
		{
			IEnumerable<TimelineDeletedItem> total;
			if (string.IsNullOrEmpty(instagramId))
				total = GetTotalDeletedEvents(0, 1000000000).Where(_ => _.User.OAccountId == username);
			else
				total = GetTotalDeletedEvents(0, 1000000000).Where(_ => _.User.OAccountId == username && _.User.OInstagramAccountUser == instagramId);

			return total?.Take(limit) ?? null;
		}
		public IEnumerable<TimelineFailedItem> GetFailedItemsForUser(string username, string instagramId = null, int limit = 30)
		{
			IEnumerable<TimelineFailedItem> total;
			if (string.IsNullOrEmpty(instagramId))
				total = GetTotalFailedEvents(0, int.MaxValue).Where(_ => _.User.OAccountId == username);
			else
				total = GetTotalFailedEvents(0, int.MaxValue).Where(_ => _.User.OAccountId == username && _.User.OInstagramAccountUser == instagramId);

			return total?.Take(limit) ?? null;
		}
		public IEnumerable<TimelineItem> GetTotalScheduledEvents(int from, int limit)
		{
			return _jobRunner.GetScheduledJobs(from, limit).Select(_=> 
			{
				var actionDetails = ((LongRunningJobOptions)_.Value.Job.Args.FirstOrDefault());
				return new TimelineItem
				{
					ItemId = _.Key,
					ActionName = actionDetails.ActionName,
					User = actionDetails.Rest.User,
					Url = actionDetails.Rest.BaseUrl,
					StartTime = _.Value.ScheduledAt,
					EnqueueTime = _.Value.EnqueueAt,
					State = _.Value.InScheduledState,
					Rest = actionDetails.Rest
					
				};
			});
		}
		public IEnumerable<TimelineFailedItem> GetTotalFailedEvents(int from, int limit)
		{
			return _jobRunner.GetFailedJobs(from, limit).Select(_ =>
			{
				var actionDetails = ((LongRunningJobOptions)_.Value.Job.Args.FirstOrDefault());
				return new TimelineFailedItem
				{
					ItemId = _.Key,
					ActionName = actionDetails.ActionName,
					User = actionDetails.Rest.User,
					Url = actionDetails.Rest.BaseUrl,
					FailedAt = _.Value.FailedAt,
					Error = _.Value.ExceptionMessage,
					State = _.Value.InFailedState
				};
			});
		}
		public IEnumerable<TimelineDeletedItem> GetTotalDeletedEvents(int from, int limit)
		{
			return _jobRunner.GetDeletedJobs(from, limit).Select(_ =>
			{
				var actionDetails = ((LongRunningJobOptions)_.Value.Job.Args.FirstOrDefault());
				return new TimelineDeletedItem
				{
					ItemId = _.Key,
					ActionName = actionDetails.ActionName,
					User = actionDetails.Rest.User,
					Url = actionDetails.Rest.BaseUrl,
					DeletedAt = _.Value.DeletedAt,
					State = _.Value.InDeletedState
				};
			});
		}
		public IEnumerable<TimelineInProgressItem> GetTotalCurrentlyRunningJobs(int from, int limit)
		{
			return _jobRunner.GetCurrentlyRunningJobs(from, limit).Select(_ =>
			{
				var actionDetails = ((LongRunningJobOptions)_.Value.Job.Args.FirstOrDefault());
				return new TimelineInProgressItem
				{
					ItemId = _.Key,
					ActionName = actionDetails.ActionName,
					User = actionDetails.Rest.User,
					Url = actionDetails.Rest.BaseUrl,
					StartedAt = _.Value.StartedAt,
					State = _.Value.InProcessingState,
				};
			});
		}
		public IEnumerable<TimelineFinishedItem> GetTotalFinishedJobs(int from, int limit)
		{
			return _jobRunner.GetFinishedJobs(from, limit).Select(_ =>
			{
				var actionDetails = ((LongRunningJobOptions)_.Value.Job.Args.FirstOrDefault());
				return new TimelineFinishedItem
				{
					ItemId = _.Key,
					ActionName = actionDetails.ActionName,
					User = actionDetails.Rest.User,
					Url = actionDetails.Rest.BaseUrl,
					SuccededAt = _.Value.SucceededAt,
					State = _.Value.InSucceededState,
					Results = _.Value.Result
				};
			});
		}
		public TimelineStatistics GetStatistics()
		{
			var statistics = _jobRunner.GetStatistics();
			return new TimelineStatistics
			{
				Deleted = statistics.Deleted,
				Scheduled = statistics.Scheduled,
				Servers = statistics.Servers,
				Succeeded = statistics.Succeeded,
				Enqueued = statistics.Enqueued,
				Failed = statistics.Failed,
				Processing = statistics.Processing,
				Queues = statistics.Queues,
				Recurring = statistics.Recurring
			};
		}
		public bool IsAnyJobsCurrentlyRunning()
		{
			return _jobRunner.IsAnyJobsCurrentlyRunning();
		}

		public bool DeleteEvent(string eventId)
		{
			return _jobRunner.DeleteJob(eventId);
		}
	}
}
