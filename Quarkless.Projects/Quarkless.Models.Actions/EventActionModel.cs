using System;
using System.Collections.Generic;
using Quarkless.Models.Actions.Enums;
using Quarkless.Models.Common.Models;

namespace Quarkless.Models.Actions
{
	public class EventBody
	{
		public Type BodyType { get; set; }
		public object Body { get; set; } //e.g: photoModel, string for id, etc
		public DateTimeOffset ExecutionTime { get; set; }

		public EventBody(object body, Type bodyType, DateTimeOffset executionTime)
		{
			this.Body = body;
			this.BodyType = bodyType;
			this.ExecutionTime = executionTime;
		}
	}
	public class EventExecuteBody
	{
		public Type BodyType { get; set; }
		public object Body { get; set; } //e.g: photoModel, string for id, etc

		public EventExecuteBody(object body, Type bodyType)
		{
			this.Body = body;
			this.BodyType = bodyType;
		}
	}
	public class EventExecute
	{
		public EventExecuteBody DataObject { get; set; }
		public UserStore User { get; set; }
	}
	public class EventActionModel
	{
		public ActionType ActionType { get; set; }
		public string ActionDescription { get; set; }
		public List<EventBody> DataObjects { get; set; }
		public UserStore User { get; set; }

		public EventActionModel(string actionDescription)
		{
			DataObjects = new List<EventBody>();
			ActionDescription = actionDescription;
		}
	}
}
