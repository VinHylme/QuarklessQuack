using System;

namespace Quarkless.Models.Timeline
{
	public struct TimelineErrorResponse
	{
		public string Message { get; set; }
		public Exception Exception { get; set; }
	}
}