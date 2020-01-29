using System;

namespace Quarkless.Models.Actions
{
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
}