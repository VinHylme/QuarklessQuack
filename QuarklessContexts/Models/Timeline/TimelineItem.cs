using System;
using System.Collections.Generic;

namespace QuarklessContexts.Models.Timeline
{
	public class TimelineDeletedItem : ITimelineItem
	{
		public string ItemId { get; set; }
		public string ActionName { get; set; }
		public DateTime? DeletedAt { get; set; }
		public bool State { get; set; }
		public UserStoreDetails User { get; set; }
		public string Url { get; set; }
	}
	public class TimelineFailedItem : ITimelineItem
	{
		public string ItemId { get; set; }
		public string ActionName { get; set; }
		public DateTime? FailedAt { get; set; }
		public string Error { get; set;}
		public bool State { get; set; }
		public UserStoreDetails User { get; set; }
		public string Url { get; set; }
	}
	public class TimelineFinishedItem : ITimelineItem
	{
		public string ItemId { get; set; }
		public string ActionName { get; set; }
		public DateTime? SuccededAt { get; set; }
		public object Results { get; set; }
		public bool State { get; set; }
		public UserStoreDetails User { get; set; }
		public string Url { get; set; }
	}
	public class TimelineInProgressItem : ITimelineItem
	{
		public string ItemId { get; set; }
		public string ActionName { get; set; }
		public DateTime? StartedAt { get; set; }
		public bool State { get; set; }
		public UserStoreDetails User { get; set; }
		public string Url { get; set; }
	}
	public class ResultBase<TResponse>
	{
		public TResponse Response { get; set; }
		public Type TimelineType { get; set; }
		public object Message { get; set; }
	}
	public interface ITimelineItem
	{

	}
	public class TimelineItemShort
	{
		public string ItemId { get; set; }
		public string ActionName { get; set; }
		public DateTime? StartTime { get; set; }
		public DateTime? EnqueueTime { get; set; }
		public bool State { get; set; }
		public string TargetId { get; set; }
		public string Body { get; set; }
	}
	public class TimelineItem : ITimelineItem
	{
		public string ItemId { get; set; }
		public string ActionName { get; set; }
		public DateTime? StartTime { get; set; }
		public DateTime? EnqueueTime { get; set; }
		public bool State { get; set; }
		public UserStoreDetails User { get; set; }
		public string Url { get; set; }
		public RestModel Rest { get; set; }
	}
	public class ItemHistory
	{
		public DateTime CreatedAt { get; set; }
		public string Reason { get ; set; }
		public string StateName { get; set; }
		public IDictionary<string,string> Data = new Dictionary<string, string>();
	}
	public class TimelineItemDetail : ITimelineItem
	{
		public string ItemId { get; set; }
		public string ActionName { get; set; }
		public DateTime? CreatedTime { get; set; }
		public DateTime? ExpireAt { get; set; }
		public DateTimeOffset ExecuteTime { get; set; }
		public UserStoreDetails User { get; set; }	
		public string Url { get; set; }
		public RestModel Rest { get; set; }
		public List<ItemHistory> History { get; set; }
	}
}
