using Quarkless.Analyser;
using Quarkless.Models.Actions.Enums.ActionTypes;
using Quarkless.Models.Actions.Interfaces;
using Quarkless.Models.Common.Objects;

namespace Quarkless.Models.Actions.Factory.Action_Options
{
	public class SendDirectMessageActionOptions : IActionOptions
	{
		public MessagingReachType MessagingReachType { get; set; }
		public MessageActionType MessageActionType { get; set; }
		public int Limit { get; set; }
		public static XRange TimeFrameSeconds { get; set; } = new XRange(300, 360);
		public readonly IPostAnalyser PostAnalyser;

		public SendDirectMessageActionOptions(IPostAnalyser postAnalyser, MessagingReachType messagingReachType = MessagingReachType.Any, int limit = 2)
		{
			PostAnalyser = postAnalyser;
			TimeFrameSeconds = new XRange(300 * limit, 360 * limit);
			this.MessagingReachType = messagingReachType;
			this.Limit = limit;
		}
	}
}