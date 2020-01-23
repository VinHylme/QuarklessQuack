﻿using System;
using Quarkless.Analyser;
using Quarkless.Models.Common.Objects;
using Quarkless.Models.Services.Automation.Enums.Actions.ActionType;
using Quarkless.Models.Services.Automation.Interfaces;

namespace Quarkless.Models.Services.Automation.Models.ActionOptions
{
	public class SendDirectMessageActionOptions : IActionOptions
	{
		public MessagingReachType MessagingReachType { get; set; }
		public MessageActionType MessageActionType { get; set; }
		public int Limit { get; set; }
		public DateTimeOffset ExecutionTime { get; set; }
		public static XRange TimeFrameSeconds { get; set; } = new XRange(300, 360);
		public IPostAnalyser PostAnalyser { get; }
		public SendDirectMessageActionOptions(DateTimeOffset executionTime,
			MessagingReachType messagingReachType, int limit, IPostAnalyser postAnalyser)
		{
			TimeFrameSeconds = new XRange(300 * limit, 360 * limit);
			this.ExecutionTime = executionTime;
			this.MessagingReachType = messagingReachType;
			this.Limit = limit;
			this.PostAnalyser = postAnalyser;
		}
	}
}