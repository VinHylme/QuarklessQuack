using Quarkless.Models.Common.Models;

namespace Quarkless.Base.Actions.Models
{
	public class EventExecute
	{
		public EventExecuteBody DataObject { get; set; }
		public UserStore User { get; set; }
	}
}