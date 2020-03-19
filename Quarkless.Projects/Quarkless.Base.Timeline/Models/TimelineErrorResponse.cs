using System;

namespace Quarkless.Base.Timeline.Models
{
	public struct TimelineErrorResponse
	{
		public string Message { get; set; }
		public Exception Exception { get; set; }
	}
}