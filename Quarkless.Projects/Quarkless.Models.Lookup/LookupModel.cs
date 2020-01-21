using System;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Lookup.Enums;

namespace Quarkless.Models.Lookup
{
	public class LookupModel
	{
		public string Id { get; set; }
		public DateTime LastModified { get; set; }
		public LookupStatus LookupStatus { get; set; } = LookupStatus.NotStarted;
		public ActionType ActionType { get; set; }
	}
}