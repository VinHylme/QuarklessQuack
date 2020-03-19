using System;
using Quarkless.Models.Common.Enums;

namespace Quarkless.Base.Actions.Models
{
	public class EventExecuteBody
	{
		public Type BodyType { get; set; }
		public object Body { get; set; }
		public ActionType ActionType { get; set; }
		public EventExecuteBody(object body, Type bodyType)
		{
			this.Body = body;
			this.BodyType = bodyType;
		}
	}
}