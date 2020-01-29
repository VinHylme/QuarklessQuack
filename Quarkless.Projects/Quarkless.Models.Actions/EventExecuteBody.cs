using System;
using Quarkless.Models.Common.Interfaces;

namespace Quarkless.Models.Actions
{
	public class EventExecuteBody
	{
		public Type BodyType { get; set; }
		public object Body { get; set; }

		public EventExecuteBody(object body, Type bodyType)
		{
			this.Body = body;
			this.BodyType = bodyType;
		}
	}
}