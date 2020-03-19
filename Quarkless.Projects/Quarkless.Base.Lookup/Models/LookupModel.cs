using System;
using Quarkless.Base.Lookup.Models.Enums;
using Quarkless.Models.Common.Enums;

namespace Quarkless.Base.Lookup.Models
{
	public class LookupModel
	{
		public string Id { get; set; }
		public string ObjectId { get; set; }
		public DateTime LastModified { get; set; }
		public LookupStatus LookupStatus { get; set; } = LookupStatus.NotStarted;
		public ActionType ActionType { get; set; }

		public LookupModel(string objectId)
		{
			ObjectId = objectId;
		}
	}
}