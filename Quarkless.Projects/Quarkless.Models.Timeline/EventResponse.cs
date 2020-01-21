using System;

namespace Quarkless.Models.Timeline
{
	public struct EventResponse
	{
		public string ItemId { get; set; }
		public string Culture { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime EnqueueAt { get; set; }
		public string State { get; set; }
		public string JsonBody { get; set; }
	}
}