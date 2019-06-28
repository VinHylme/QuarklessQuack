using System;
using System.Collections.Generic;
using System.Text;

namespace QuarklessContexts.Models.Timeline
{
	public class TimelineDeletedItem
	{
		public string ItemId { get; set; }
		public string ActionName { get; set; }
		public DateTime? DeletedAt { get; set; }
		public bool State { get; set; }
		public UserStore User { get; set; }
		public string Url { get; set; }
	}
	public class TimelineFailedItem
	{
		public string ItemId { get; set; }
		public string ActionName { get; set; }
		public DateTime? FailedAt { get; set; }
		public string Error { get; set;}
		public bool State { get; set; }
		public UserStore User { get; set; }
		public string Url { get; set; }
	}
	public class TimelineFinishedItem
	{
		public string ItemId { get; set; }
		public string ActionName { get; set; }
		public DateTime? SuccededAt { get; set; }
		public object Results { get; set; }
		public bool State { get; set; }
		public UserStore User { get; set; }
		public string Url { get; set; }
	}
	public class TimelineInProgressItem
	{
		public string ItemId { get; set; }
		public string ActionName { get; set; }
		public DateTime? StartedAt { get; set; }
		public bool State { get; set; }
		public UserStore User { get; set; }
		public string Url { get; set; }
	}
	public class ResultBase<TResponse>
	{
		public TResponse Response { get; set; }
		public Type TimelineType { get; set; }
		public object Message { get; set; }
	}
	public class TimelineItem
	{
		public string ItemId { get; set; }
		public string ActionName { get; set; }
		public DateTime? StartTime { get; set; }
		public DateTime? EnqueueTime { get; set; }
		public bool State { get; set; }
		public UserStore User { get; set; }
		public string Url { get; set; }
	}
	public class ItemHistory
	{
		public DateTime CreatedAt { get; set; }
		public string Reason { get ; set; }
		public string StateName { get; set; }
		public IDictionary<string,string> Data = new Dictionary<string, string>();
	}
	public class TimelineItemDetail
	{
		public string ItemId { get; set; }
		public string ActionName { get; set; }
		public DateTime? CreatedTime { get; set; }
		public DateTime? ExpireAt { get; set; }
		public DateTimeOffset ExecuteTime { get; set; }
		public UserStore User { get; set; }	
		public string Url { get; set; }
		public List<ItemHistory> History { get; set; }
	}
}
