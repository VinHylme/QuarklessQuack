using System;
using QuarklessContexts.Enums;

namespace QuarklessContexts.Models.LookupModels
{
	public enum LookupStatus
	{
		NotStarted,
		Pending,
		Completed,
		Failed
	}
	public class LookupModel
	{
		public string Id { get; set; }
		public DateTime LastModified { get; set; }
		public LookupStatus LookupStatus { get; set; } = LookupStatus.NotStarted;
		public ActionType ActionType { get; set; }
	}
}
