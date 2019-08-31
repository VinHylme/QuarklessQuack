using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QuarklessContexts.Enums;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models.MediaModels;
using QuarklessContexts.Models.Timeline;
using QuarklessLogic.Handlers.RequestBuilder.RequestBuilder;
using QuarklessLogic.QueueLogic.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MoreLinq;

namespace QuarklessLogic.ServicesLogic.TimelineServiceLogic.TimelineLogic
{
	public enum TimelineDateType
	{
		Backwards,
		Forward	
	}
	public class TimelineLogic : ITimelineLogic
	{
		private readonly ITaskService _taskService;
		private readonly IRequestBuilder _requestBuilder;
		public TimelineLogic(ITaskService taskService, IRequestBuilder requestBuilder)
		{
			_taskService = taskService;
			_requestBuilder = requestBuilder;
		}
		#region Add Event To Queue
		public string AddEventToTimeline(string actionName, RestModel restBody, DateTimeOffset executeTime)
		{
			restBody.RequestHeaders = _requestBuilder.DefaultHeaders(restBody.User.OInstagramAccountUser).ToList();

			var eventId = _taskService.ScheduleEvent(actionName, restBody, executeTime);
			return eventId;
		}
		public T ParseJsonObject<T>(string json) where T : class, new()
		{
			JObject jobject = JObject.Parse(json);
			return JsonConvert.DeserializeObject<T>(jobject.ToString());
		}
		public string UpdateEvent(UpdateTimelineItemRequest updateTimelineItemRequest)
		{
			try { 
				var job = _taskService.GetEvent(updateTimelineItemRequest.EventId);
				job.ExecuteTime = updateTimelineItemRequest.Time;
			
				var object_ = JsonConvert.DeserializeObject(job.Rest.JsonBody, 
					updateTimelineItemRequest.MediaType == 0 ? typeof(UploadPhotoModel) 
					: updateTimelineItemRequest.MediaType == 1 ? typeof(UploadVideoModel) 
					: updateTimelineItemRequest.MediaType == 2 ? typeof(UploadAlbumModel) : typeof(object)
					);

				object_.GetProp("MediaInfo").SetValue(object_, new MediaInfo
				{
					Caption = updateTimelineItemRequest.Caption,
					Credit = updateTimelineItemRequest.Credit,
					Hashtags = updateTimelineItemRequest.Hashtags
				});
				object_.GetProp("Location").SetValue(object_, updateTimelineItemRequest.Location);

				job.Rest.JsonBody = JsonConvert.SerializeObject(object_);

				if (DeleteEvent(updateTimelineItemRequest.EventId)) { 
					var res = AddEventToTimeline(job.ActionName, job.Rest, job.ExecuteTime);
					if(!string.IsNullOrEmpty(res))
						return res;
				}
				return null;
			}
			catch(Exception ee)
			{
				Console.WriteLine(ee.Message);
				return null;
			}
		}
		public void ExecuteNow(string eventId)
		{
			_taskService.ExecuteNow(eventId);
		}
		#endregion

		#region Get Specified UserAction
		public IEnumerable<TimelineItem> GetScheduledEventsForUserForAction(string actionName, string username, string instaId = null, int limit = 100)
		{
			return GetScheduledEventsForUser(username, instaId, limit: limit).Where(_ => _.ActionName.Split('_')[0].ToLower().Equals(actionName.ToLower()));
		}
		#endregion
		#region GET BY USING DATES
		public IEnumerable<ResultBase<TimelineItem>> GetAllEventsForUserByAction(string actionName, string userName,
			DateTime startDate, DateTime? endDate = null, string instaId = null, int limit = 1000, TimelineDateType timelineDateType = TimelineDateType.Backwards)
		{
			return GetAllEventsForUser(userName, startDate, endDate, instaId, limit, timelineDateType).Where(_ => _?.Response?.ActionName?.Split('_')?[0]?.ToLower() == (actionName.ToLower()));
		}
		public IEnumerable<TimelineItem> GetScheduledEventsForUserByDate(string username, DateTime date, DateTime? endDate = null,
			string instaId = null, int limit = 100, TimelineDateType timelineDateType = TimelineDateType.Backwards)
		{
			switch (timelineDateType)
			{
				case TimelineDateType.Backwards:
					if (endDate == null)
					{
						endDate = date.AddDays(-1).AddTicks(1);
					}
					var eventsB = GetScheduledEventsForUser(username, instaId, limit: limit)
						.Where(_ => _.EnqueueTime <= date && _.EnqueueTime >= endDate);
					return eventsB;
				case TimelineDateType.Forward:
					if (endDate == null)
					{
						endDate = date.AddDays(1).AddTicks(-1);
					}
					var eventsF = GetScheduledEventsForUser(username, instaId, limit: limit)
						.Where(_ => _.EnqueueTime >= date && _.EnqueueTime <= endDate);
					return eventsF;
			}
			return null;
		}
		public IEnumerable<TimelineFinishedItem> GetFinishedEventsForUserByDate(string username, DateTime date, DateTime? endDate = null,
			string instaId = null, int limit = 100, TimelineDateType timelineDateType = TimelineDateType.Backwards)
		{
			switch (timelineDateType)
			{
				case TimelineDateType.Backwards:
					if (endDate == null)
					{
						endDate = date.AddDays(-1).AddTicks(1);
					}
					var eventsB = GetFinishedEventsForUser(username, instaId, limit: limit)
						.Where(_ => _?.SuccededAt <= date && _?.SuccededAt >= endDate);
					return eventsB;
				case TimelineDateType.Forward:
					if (endDate == null)
					{
						endDate = date.AddDays(1).AddTicks(-1);
					}
					var eventsF = GetFinishedEventsForUser(username, instaId, limit: limit);
						
					return eventsF.Where(_ => _?.SuccededAt >= date && _?.SuccededAt <= endDate);
			}
			return null;
		}
		public IEnumerable<TimelineInProgressItem> GetCurrentlyRunningEventsForUserByDate(string username, DateTime date, DateTime? endDate = null,
			int limit = 100, string instaid = null, TimelineDateType timelineDateType = TimelineDateType.Backwards)
		{
			switch (timelineDateType)
			{
				case TimelineDateType.Backwards:
					if (endDate == null)
					{
						endDate = date.AddDays(-1).AddTicks(1);
					}
					var eventsB = GetCurrentlyRunningEventsForUser(username, instaid, limit: limit)
						.Where(_ => _.StartedAt <= date && _.StartedAt >= endDate);
					return eventsB;
				case TimelineDateType.Forward:
					if (endDate == null)
					{
						endDate = date.AddDays(1).AddTicks(-1);
					}
					var eventsF = GetCurrentlyRunningEventsForUser(username, instaid, limit: limit)
						.Where(_ => _.StartedAt >= date && _.StartedAt <= endDate);
					return eventsF;
			}
			return null;
		}
		public IEnumerable<TimelineDeletedItem> GetDeletedEventsForUserByDate(string username, DateTime date, DateTime? endDate = null,
			string instaId = null, int limit = 100, TimelineDateType timelineDateType = TimelineDateType.Backwards)
		{
			switch (timelineDateType)
			{
				case TimelineDateType.Backwards:
					if (endDate == null)
					{
						endDate = date.AddDays(-1).AddTicks(1);
					}
					var eventsB = GetDeletedEventsForUser(username, instaId, limit: limit)
						.Where(_ => _.DeletedAt <= date && _.DeletedAt >= endDate);
					return eventsB;
				case TimelineDateType.Forward:
					if (endDate == null)
					{
						endDate = date.AddDays(1).AddTicks(-1);
					}
					var eventsF = GetDeletedEventsForUser(username, instaId, limit: limit)
						.Where(_ => _.DeletedAt >= date && _.DeletedAt <= endDate);
					return eventsF;
			}
			return null;
		}
		public IEnumerable<TimelineFailedItem> GetFailedEventsForUserByDate(string username, DateTime date, DateTime? endDate = null,
			string instaId = null, int limit = 100, TimelineDateType timelineDateType = TimelineDateType.Backwards)
		{
			switch (timelineDateType)
			{
				case TimelineDateType.Backwards:
					if (endDate == null)
					{
						endDate = date.AddDays(-1).AddTicks(1);
					}
					var eventsB = GetFailedEventsForUser(username, instaId, limit: limit)
						.Where(_ => _.FailedAt <= date && _.FailedAt >= endDate);
					return eventsB;
				case TimelineDateType.Forward:
					if (endDate == null)
					{
						endDate = date.AddDays(1).AddTicks(-1);
					}
					var eventsF = GetFailedEventsForUser(username, instaId, limit: limit)
						.Where(_ => _.FailedAt >= date && _.FailedAt <= endDate);
					return eventsF;
			}
			return null;
		}
		public IEnumerable<TimelineItem> GetScheduledEventsForUserForActionByDate(string username, string actionName, DateTime date,
			string instaId = null, DateTime? endDate = null, int limit = 30, TimelineDateType timelineDateType = TimelineDateType.Backwards)
		{
			switch (timelineDateType)
			{
				case TimelineDateType.Backwards:
					if (endDate == null)
					{
						endDate = date.AddDays(-1).AddTicks(1);
					}
					var eventsB = GetScheduledEventsForUser(username, instaId, limit: limit)
						.Where(_ => (_.EnqueueTime <= date && _.EnqueueTime >= endDate) && _?.ActionName?.Split('_')?[0]?.ToLower() == (actionName.ToLower()));
					return eventsB;
				case TimelineDateType.Forward:
					if (endDate == null)
					{
						endDate = date.AddDays(1).AddTicks(-1);
					}
					var eventsF = GetScheduledEventsForUser(username, instaId, limit: limit)
						.Where(_ => (_.EnqueueTime >= date && _.EnqueueTime <= endDate) && _?.ActionName?.Split('_')?[0]?.ToLower() == (actionName.ToLower()));
					return eventsF;
			}
			return null;
		}
		public IEnumerable<ResultBase<TimelineItemShort>> ShortGetAllEventsForUser(string userName, DateTime startDate, DateTime? endDate = null,
		string instaId = null, int limit = 1000, TimelineDateType timelineDateType = TimelineDateType.Backwards)
		{
			List<ResultBase<TimelineItemShort>> totalEvents = new List<ResultBase<TimelineItemShort>>();
			totalEvents.AddRange(GetScheduledEventsForUserByDate(userName, startDate, endDate, instaId, limit, timelineDateType).Select(_ => new ResultBase<TimelineItemShort>
			{
				Response = new TimelineItemShort
				{
					ActionName = _.ActionName,
					EnqueueTime = _.EnqueueTime,
					ItemId = _.ItemId,
					StartTime = _.StartTime,
					State = _.State,
					Body = _.Rest.JsonBody,
					TargetId = Regex.Match(_.Url.Split('/').Last(),@"\d+").Value
				},
				TimelineType = typeof(TimelineItemShort)
			}));
			return totalEvents;
		}
		public IEnumerable<TimelineItemShort> GetScheduledPosts(string username, string instagramId, int limit = 1000)
		{
			var res = GetScheduledEventsForUserForAction(ActionType.CreatePost.GetDescription(), username, instagramId, limit).ToList();

			return res.Select(s => new TimelineItemShort
			{
				ActionName = s.ActionName,
				StartTime = s.StartTime,
				State = s.State,
				Body = s.Rest.JsonBody,
				EnqueueTime = s.EnqueueTime,
				ItemId = s.ItemId,
				TargetId = Regex.Match(s.Url.Split('/').Last(), @"\d+").Value
			}).Distinct();
		}
		public IEnumerable<ResultBase<TimelineItem>> GetAllEventsForUser(string userName, DateTime startDate, DateTime? endDate = null,
		string instaId = null, int limit = 1000, TimelineDateType timelineDateType = TimelineDateType.Backwards)
		{
			List<ResultBase<TimelineItem>> totalEvents = new List<ResultBase<TimelineItem>>();
			totalEvents.AddRange(GetScheduledEventsForUserByDate(userName, startDate, endDate, instaId, limit, timelineDateType).Select(_ => new ResultBase<TimelineItem>
			{
				Response = new TimelineItem
				{
					ActionName = _.ActionName,
					EnqueueTime = _.EnqueueTime,
					ItemId = _.ItemId,
					StartTime = _.StartTime,
					State = _.State,
					Url = _.Url,
					User = _.User,
					Rest = _.Rest
				},
				TimelineType = typeof(TimelineItem)
			}));
			/*totalEvents.AddRange(GetFinishedEventsForUserByDate(userName, startDate, endDate, instaId, limit, timelineDateType).Select(_ => new ResultBase<TimelineItem>
			{
				Response = new TimelineItem
				{
					ActionName = _.ActionName,
					EnqueueTime = _.SuccededAt,
					ItemId = _.ItemId,
					State = _.State,
					Url = _.Url,
					User = _.User
				},
				Message = _.Results,
				TimelineType = typeof(TimelineFinishedItem)
			}));*/
			/*
			totalEvents.AddRange(GetCurrentlyRunningEventsForUserByDate(userName, startDate, endDate, limit, instaId, timelineDateType).Select(_ => new ResultBase<TimelineItem>
			{
				Response = new TimelineItem
				{
					ActionName = _.ActionName,
					EnqueueTime = _.StartedAt,
					ItemId = _.ItemId,
					State = _.State,
					Url = _.Url,
					User = _.User
				},
				TimelineType = typeof(TimelineInProgressItem)
			}));
			*/
			/*
			totalEvents.AddRange(GetDeletedEventsForUserByDate(userName, startDate, endDate, instaId, limit, timelineDateType).Select(_ => new ResultBase<TimelineItem>
			{
				Response = new TimelineItem
				{
					ActionName = _.ActionName,
					EnqueueTime = _.DeletedAt,
					ItemId = _.ItemId,
					State = _.State,
					Url = _.Url,
					User = _.User
				},
				TimelineType = typeof(TimelineDeletedItem)
			}));
			totalEvents.AddRange(GetFailedEventsForUserByDate(userName, startDate, endDate, instaId, limit, timelineDateType).Select(_ => new ResultBase<TimelineItem>
			{
				Response = new TimelineItem
				{
					ActionName = _.ActionName,
					EnqueueTime = _.FailedAt,
					ItemId = _.ItemId,
					State = _.State,
					Url = _.Url,
					User = _.User
				},
				Message = _.Error,
				TimelineType = typeof(TimelineFailedItem)
			}));
			*/
			return totalEvents;
		}

		#endregion
		#region GETTING EVENT DETAILS FOR THE USER
		public IEnumerable<TimelineItem> GetScheduledEventsForUser(string username, string instagramId = null, int limit = 30)
		{
			return _taskService.GetScheduledItemsForUser(username, instagramId, limit);
		}
		public IEnumerable<TimelineFinishedItem> GetFinishedEventsForUser(string username, string instagramId = null, int limit = 30)
		{
			return _taskService.GetFinishedItemsForUser(username, instagramId, limit);
		}
		public IEnumerable<TimelineInProgressItem> GetCurrentlyRunningEventsForUser(string username, string instagramId = null, int limit = 30)
		{
			return _taskService.GetCurrentlyRunningItemsForUser(username, instagramId, limit);
		}
		public IEnumerable<TimelineFailedItem> GetFailedEventsForUser(string username, string instagramId = null, int limit = 30)
		{
			return _taskService.GetFailedItemsForUser(username, instagramId, limit);
		}
		public IEnumerable<TimelineDeletedItem> GetDeletedEventsForUser(string username, string instagramId = null, int limit = 30)
		{
			return _taskService.GetDeletedItemsForUser(username, instagramId, limit);
		}
		#endregion
		#region GET BY SPECIFIED JOB ID FOR ANY USER (ADMIN) BUT CAN BE USED FOR USER LEVEL TOO (BECAREFUL)
		public TimelineItemDetail GetEventDetail(string eventId)
		{
			return _taskService.GetEvent(eventId);
		}
		#endregion
		#region ADMIN TIMELINE OPTIONS
		public TimelineStatistics GetTimelineStatistics()
		{
			return _taskService.GetStatistics();
		}
		public IEnumerable<TimelineInProgressItem> GetTotalCurrentlyRunningEvents(int from, int limit)
		{
			return _taskService.GetTotalCurrentlyRunningJobs(from, limit);
		}
		public IEnumerable<TimelineFinishedItem> GetTotalFinishedEvents(int from, int limit)
		{
			return _taskService.GetTotalFinishedJobs(from, limit);
		}
		public IEnumerable<TimelineDeletedItem> GetTotalDeletedEvents(int from, int limit)
		{
			return _taskService.GetTotalDeletedEvents(from, limit);
		}
		public IEnumerable<TimelineFailedItem> GetTotalFailedEvents(int from, int limit)
		{
			return _taskService.GetTotalFailedEvents(from, limit);
		}
		public IEnumerable<TimelineItem> GetTotalScheduledEvents(int from, int limit)
		{
			return _taskService.GetTotalScheduledEvents(from, limit);
		}
		public bool IsAnyEventsCurrentlyRunning()
		{
			return _taskService.IsAnyJobsCurrentlyRunning();
		}

		public bool DeleteEvent(string eventId)
		{
			return _taskService.DeleteEvent(eventId);
		}
		#endregion
	}
}
