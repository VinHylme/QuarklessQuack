using System.Collections.Generic;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Common.Models;

namespace Quarkless.Base.Actions.Models
{
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
