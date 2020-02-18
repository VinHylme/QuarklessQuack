using System;
using System.Collections.Generic;
using Quarkless.Models.Common.Enums;

namespace Quarkless.Models.Timeline
{
	public class EventModel
	{
		public ActionType ActionType { get; set; }
		public List<object> DataObjects { get; set; }
		public UserStoreDetails User { get; set; }
		public DateTimeOffset ExecutionTime { get; set; }
	}
}