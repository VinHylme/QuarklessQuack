using Quarkless.Models.Common.Models;

namespace Quarkless.Models.Actions
{
	public class EventExecute
	{
		public EventExecuteBody DataObject { get; set; }
		public UserStore User { get; set; }
	}
}