using System;

namespace Quarkless.Models.Timeline
{
	public class ResultBase<TResponse>
	{
		public TResponse Response { get; set; }
		public Type TimelineType { get; set; }
		public object Message { get; set; }
	}
}