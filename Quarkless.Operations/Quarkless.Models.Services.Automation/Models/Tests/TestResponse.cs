using System.Collections.Generic;
using Quarkless.Models.Actions;
using Quarkless.Models.Common.Models.Carriers;

namespace Quarkless.Models.Services.Automation.Models.Tests
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
