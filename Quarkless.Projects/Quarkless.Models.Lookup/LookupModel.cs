using System;
using Newtonsoft.Json;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Lookup.Enums;

namespace Quarkless.Models.Lookup
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