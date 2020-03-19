using System;

namespace Quarkless.Base.Actions.Models
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
}