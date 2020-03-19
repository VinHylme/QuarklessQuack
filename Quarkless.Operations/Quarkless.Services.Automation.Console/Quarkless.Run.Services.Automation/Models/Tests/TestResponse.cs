using System.Collections.Generic;
using Quarkless.Base.Actions.Models;
using Quarkless.Models.Common.Models.Carriers;

namespace Quarkless.Run.Services.Automation.Models.Tests
{
	public class TestResponse
	{
		public bool IsSuccessful { get; set; }
		public bool PassedInActionBuild { get; set; }
		public bool PassedInActionExecute { get; set; }
		public List<EventBody> Items { get; set; }
		public ErrorResponse Error { get; set; }
	}
}
