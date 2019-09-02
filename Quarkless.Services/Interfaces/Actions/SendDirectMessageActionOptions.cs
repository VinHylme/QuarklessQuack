using System;
using Quarkless.Services.ActionBuilders.EngageActions;
using QuarklessContexts.Models;

namespace Quarkless.Services.Interfaces.Actions
{
	public class SendDirectMessageActionOptions : IActionOptions
	{
		public MessagingReachType MessagingReachType { get; set; }
		public MessageActionType MessageActionType { get; set; }
		public int Limit { get; set; }
		public DateTimeOffset ExecutionTime { get; set; }
		public static Range TimeFrameSeconds { get; set; } = new Range(300, 360);
		public SendDirectMessageActionOptions(DateTimeOffset executionTime, 
			MessagingReachType messagingReachType, int limit)
		{
			TimeFrameSeconds = new Range(300*limit,360*limit);
			this.ExecutionTime = executionTime;
			this.MessagingReachType = messagingReachType;
			this.Limit = limit;
		}
	}
}
