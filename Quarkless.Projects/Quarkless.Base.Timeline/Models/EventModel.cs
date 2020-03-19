using System;
using System.Collections.Generic;
using Quarkless.Common.Timeline.Models;
using Quarkless.Models.Common.Enums;

namespace Quarkless.Base.Timeline.Models
{
	public class EventModel
	{
		public ActionType ActionType { get; set; }
		public List<object> DataObjects { get; set; }
		public UserStoreDetails User { get; set; }
		public DateTimeOffset ExecutionTime { get; set; }
	}
}