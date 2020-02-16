using System;
using System.Collections.Generic;
using System.Linq;
using Quarkless.Logic.Timeline.TaskScheduler.Extensions;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Timeline;
using Quarkless.Models.Timeline.Interfaces.TaskScheduler;
using Quarkless.Models.Timeline.TaskScheduler;

namespace Quarkless.Logic.Timeline.TaskScheduler
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

		public void DeleteAllFailedJobs()
		{
			var failedJobs = GetTotalFailedEvents(0, 2000);
			foreach (var timelineFailedItem in failedJobs)
			{
				DeleteEvent(timelineFailedItem.ItemId);
			}

			// var deletedJobs = GetTotalDeletedEvents(0, 2000);
			// foreach (var timelineDeletedItem in deletedJobs)
			// {
			// 	DeleteEvent(timelineDeletedItem.ItemId);
			// }
		}

		public string ScheduleEvent(EventActionOptions eventAction)
		{
			return _jobRunner.AddJob(op =>
			{
				op.ActionType = eventAction.ActionType;
				op.ActionDescription = eventAction.ActionDescription;
				op.User = eventAction.User;
				op.ExecutionTime = eventAction.ExecutionTime;
				op.DataObject = eventAction.DataObject;
			});
		}
		public TimelineItemDetail GetEvent(string itemId)
		{
			var job = _jobRunner.GetJobDetails(itemId);
			var actionDetails = (EventActionOptions)job.Job.Args.First();

			return new TimelineItemDetail { 
				ItemId = itemId,
				ActionType = (ActionType) actionDetails.ActionType,
				ActionDescription = actionDetails.ActionDescription,
				CreatedTime = job.CreatedAt,
				ExpireAt = job.ExpireAt,
				ExecuteTime = actionDetails.ExecutionTime,
				User = actionDetails.User,
				EventBody = actionDetails.DataObject,
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
			var total = string.IsNullOrEmpty(instagramId)
				? GetTotalScheduledEvents(0,int.MaxValue).Where(_ => _.User.AccountId == username) 
				: GetTotalScheduledEvents(0, int.MaxValue).Where(_ => _.User.AccountId == username && _.User.InstagramAccountUser == instagramId);
			
			return total.Take(limit);
		}
		public IEnumerable<TimelineInProgressItem> GetCurrentlyRunningItemsForUser(string username, string instagramId = null, int limit = 30)
		{
			IEnumerable<TimelineInProgressItem> total;
			total = string.IsNullOrEmpty(instagramId) 
				? GetTotalCurrentlyRunningJobs(0, int.MaxValue).Where(_ => _.User.AccountId == username) 
				: GetTotalCurrentlyRunningJobs(0, int.MaxValue).Where(_ => _.User.AccountId == username && _.User.InstagramAccountUser == instagramId);

			return total?.Take(limit) ?? null;
		}
		public IEnumerable<TimelineFinishedItem> GetFinishedItemsForUser(string username, string instagramId = null, int limit = 30)
		{
			IEnumerable<TimelineFinishedItem> total;
			total = string.IsNullOrEmpty(instagramId) 
				? GetTotalFinishedJobs(0, int.MaxValue).Where(_ => _.User.AccountId == username) 
				: GetTotalFinishedJobs(0, int.MaxValue).Where(_ => _.User.AccountId == username && _.User.InstagramAccountUser == instagramId);

			return total?.Take(limit) ?? null;
		}
		public IEnumerable<TimelineDeletedItem> GetDeletedItemsForUser(string username, string instagramId = null, int limit = 30)
		{
			IEnumerable<TimelineDeletedItem> total;
			total = string.IsNullOrEmpty(instagramId) 
				? GetTotalDeletedEvents(0, 1000000000).Where(_ => _.User.AccountId == username) 
				: GetTotalDeletedEvents(0, 1000000000).Where(_ => _.User.AccountId == username 
				&& _.User.InstagramAccountUser == instagramId);

			return total?.Take(limit) ?? null;
		}
		public IEnumerable<TimelineFailedItem> GetFailedItemsForUser(string username, string instagramId = null, int limit = 30)
		{
			IEnumerable<TimelineFailedItem> total;
			total = string.IsNullOrEmpty(instagramId) 
				? GetTotalFailedEvents(0, int.MaxValue).Where(_ => _.User.AccountId == username) 
				: GetTotalFailedEvents(0, int.MaxValue).Where(_ => _.User.AccountId == username 
				&& _.User.InstagramAccountUser == instagramId);
			return total?.Take(limit) ?? null;
		}
		public IEnumerable<TimelineItem> GetTotalScheduledEvents(int from, int limit)
		{
			var res = _jobRunner.GetScheduledJobs(from, limit);
			if (res == null) yield break;
			foreach (var total in res)
			{
				var actionDetails = (EventActionOptions) total.Value?.Job?.Args?.FirstOrDefault();
				if (actionDetails != null)
				{
					yield return new TimelineItem
					{
						ItemId = total.Key,
						ActionType = (ActionType) actionDetails.ActionType,
						ActionDescription =  actionDetails.ActionDescription,
						User = actionDetails.User,
						EventBody = actionDetails.DataObject,
						StartTime = total.Value?.ScheduledAt,
						EnqueueTime = total.Value?.EnqueueAt,
						State = total.Value.InScheduledState,
					};
				}
			}
		}
		public IEnumerable<TimelineFailedItem> GetTotalFailedEvents(int from, int limit)
		{
			var res = _jobRunner.GetFailedJobs(from, limit);
			if (res == null) yield break;
			foreach (var total in res)
			{
				var actionDetails = (EventActionOptions) total.Value?.Job?.Args?.FirstOrDefault();
				if (actionDetails != null)
				{
					yield return new TimelineFailedItem
					{
						ItemId = total.Key,
						ActionType = (ActionType) actionDetails.ActionType,
						ActionDescription = actionDetails.ActionDescription,
						User = actionDetails.User,
						EventBody = actionDetails.DataObject,
						FailedAt = total.Value?.FailedAt,
						State = total.Value.InFailedState,
						Error = total.Value?.Reason
					};
				}
			}
		}
		public IEnumerable<TimelineDeletedItem> GetTotalDeletedEvents(int from, int limit)
		{
			var res = _jobRunner.GetDeletedJobs(from, limit);
			if (res == null) yield break;
			foreach (var total in res)
			{
				var actionDetails = (EventActionOptions) total.Value?.Job?.Args?.FirstOrDefault();
				if (actionDetails != null)
				{
					yield return new TimelineDeletedItem
					{
						ItemId = total.Key,
						ActionType = (ActionType) actionDetails.ActionType,
						ActionDescription = actionDetails.ActionDescription,
						User = actionDetails.User,
						EventBody = actionDetails.DataObject,
						DeletedAt = total.Value?.DeletedAt,
						State = total.Value.InDeletedState
					};
				}
			}
		}
		public IEnumerable<TimelineInProgressItem> GetTotalCurrentlyRunningJobs(int from, int limit)
		{
			var res = _jobRunner.GetCurrentlyRunningJobs(from, limit);
			return res?.Select(_ =>
			{
				if (_.Value != null) { 
					var actionDetails = (EventActionOptions)_.Value.Job.Args.First();
					return new TimelineInProgressItem
					{
						ItemId = _.Key,
						ActionType = (ActionType)actionDetails.ActionType,
						ActionDescription = actionDetails.ActionDescription,
						User = actionDetails.User,
						EventBody = actionDetails.DataObject,
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
		public IEnumerable<TimelineFinishedItem> GetTotalFinishedJobs(int from, int limit)
		{
			var res = _jobRunner.GetFinishedJobs(from,limit).Where(s=>s.Value!=null);
			foreach(var total in res)
			{
				var actionDetails = (EventActionOptions) total.Value?.Job?.Args?.First();
				if (actionDetails != null)
				{
					yield return new TimelineFinishedItem
					{
						ItemId = total.Key,
						ActionType = (ActionType)actionDetails.ActionType,
						ActionDescription = actionDetails.ActionDescription,
						User = actionDetails.User,
						EventBody = actionDetails.DataObject,
						SuccessAt = total.Value?.SucceededAt,
						State = total.Value.InSucceededState,
						Results = total.Value?.Result
					};
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
