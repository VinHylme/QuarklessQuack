﻿using QuarklessContexts.JobClass;
using QuarklessContexts.Models.Timeline;
using QuarklessLogic.QueueLogic.Jobs.JobOptions;
using QuarklessLogic.QueueLogic.Jobs.JobTypes;
using QuarklessRepositories.RedisRepository.TimelineJobRedis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QuarklessLogic.QueueLogic.Services
{
	public class TaskService : ITaskService
	{
		private readonly IJobRunner _jobRunner;
		private readonly ITimelineJobRepository _timelineJobRepository;
		public TaskService(IJobRunner jobRunner, ITimelineJobRepository timelineJobRepository)
		{
			_jobRunner = jobRunner;
			_timelineJobRepository = timelineJobRepository;
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
				Rest = actionDetails.Rest,
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
			var res = _jobRunner.GetScheduledJobs(from, limit);
			if (res != null)
			{
				foreach (var total in res)
				{
					if (total.Value != null)
					{
						var actionDetails = ((LongRunningJobOptions)total.Value?.Job?.Args?.FirstOrDefault());
						if (actionDetails != null)
						{
							yield return new TimelineItem
							{
								ItemId = total.Key,
								ActionName = actionDetails?.ActionName,
								User = actionDetails?.Rest?.User,
								Url = actionDetails?.Rest?.BaseUrl,
								StartTime = total.Value?.ScheduledAt,
								EnqueueTime = total.Value?.EnqueueAt,
								State = total.Value.InScheduledState,
								Rest = actionDetails?.Rest
							};
						}
					}
				}
			}
		}
		public IEnumerable<TimelineFailedItem> GetTotalFailedEvents(int from, int limit)
		{
			var res = _jobRunner.GetFailedJobs(from, limit);
			if (res != null)
			{
				foreach (var total in res)
				{
					if (total.Value != null)
					{
						var actionDetails = ((LongRunningJobOptions)total.Value?.Job?.Args?.FirstOrDefault());
						if (actionDetails != null)
						{
							yield return new TimelineFailedItem
							{
								ItemId = total.Key,
								ActionName = actionDetails?.ActionName,
								User = actionDetails?.Rest?.User,
								Url = actionDetails?.Rest?.BaseUrl,
								FailedAt = total.Value?.FailedAt,
								State = total.Value.InFailedState,
								Error = total.Value?.Reason
							};
						}
					}
				}
			}
		}
		public IEnumerable<TimelineDeletedItem> GetTotalDeletedEvents(int from, int limit)
		{
			var res = _jobRunner.GetDeletedJobs(from, limit);
			if (res != null)
			{
				foreach (var total in res)
				{
					if (total.Value != null)
					{
						var actionDetails = ((LongRunningJobOptions)total.Value?.Job?.Args?.FirstOrDefault());
						if (actionDetails != null)
						{
							yield return new TimelineDeletedItem
							{
								ItemId = total.Key,
								ActionName = actionDetails?.ActionName,
								User = actionDetails?.Rest?.User,
								Url = actionDetails?.Rest?.BaseUrl,
								DeletedAt = total.Value?.DeletedAt,
								State = total.Value.InDeletedState
							};
						}
					}
				}
			}
		}
		public IEnumerable<TimelineInProgressItem> GetTotalCurrentlyRunningJobs(int from, int limit)
		{
			var res = _jobRunner.GetCurrentlyRunningJobs(from, limit);
			if (res != null)
			{
				return res.Select(_ =>
				{
					if (_.Value != null) { 
						var actionDetails = ((LongRunningJobOptions)_.Value.Job.Args.FirstOrDefault());
						return new TimelineInProgressItem
						{
							ItemId = _.Key,
							ActionName = actionDetails?.ActionName,
							User = actionDetails?.Rest?.User,
							Url = actionDetails?.Rest?.BaseUrl,
							StartedAt = _.Value.StartedAt,
							State = _.Value.InProcessingState,
						};
					}
					else
					{
						return null;
					}
				});
			}
			else
			{
				return null;
			}
		}
		public IEnumerable<TimelineFinishedItem> GetTotalFinishedJobs(int from, int limit)
		{
			var res = _jobRunner.GetFinishedJobs(from,limit).Where(s=>s.Value!=null);
			if (res != null)
			{
				foreach(var total in res)
				{
					if (total.Value != null)
					{
						var actionDetails = ((LongRunningJobOptions)total.Value?.Job?.Args?.FirstOrDefault());
						if (actionDetails != null)
						{
							yield return new TimelineFinishedItem
							{
								ItemId = total.Key,
								ActionName = actionDetails?.ActionName,
								User = actionDetails?.Rest?.User,
								Url = actionDetails?.Rest?.BaseUrl,
								SuccededAt = total.Value?.SucceededAt,
								State = total.Value.InSucceededState,
								Results = total.Value?.Result
							};
						}
					}
				}
			}
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

		public void ExecuteNow(string jobid)
		{
			_jobRunner.ExecuteNow(jobid);
		}
	}
}