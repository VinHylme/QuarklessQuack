using System;
using Quarkless.Models.Common.Models;
using Quarkless.Models.Timeline.Interfaces.TaskScheduler;

namespace Quarkless.Models.Timeline.TaskScheduler
{
	public class EventActionOptions: IJobOptions
	{
		public class EventBody
		{
			public Type BodyType { get; set; }
			public object Body { get; set; }
		}
		public int ActionType { get; set; }
		public string ActionDescription { get; set; }
		public EventBody DataObject { get; set; }
		public UserStore User { get; set; }
		public DateTimeOffset ExecutionTime { get; set; }
	}
}