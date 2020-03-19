using System;
using Quarkless.Base.Notification.Models.Enums;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Common.Models.Resolver;

namespace Quarkless.Base.Notification.Models.Extensions
{
	public static class NotificationExtensions
	{

		public static NotificationTimelineAction CreateTimelineNotificationObject(string accountId, 
			string instagramAccountId, string assetUrl, MediaShort media, string message, ActionType actionType,
			int responseType, string responseMessage, TimelineEventItemStatus timelineStatus)
		{
			return new NotificationTimelineAction
			{
				Notification = new NotificationObject
				{
					AccountId = accountId,
					InstagramAccountId = instagramAccountId,
					AssetUrl = assetUrl,
					CreatedAt = DateTime.UtcNow,
					Message = message,
					Status = NotificationStatus.Pending
				},
				ActionType = actionType,
				ResponseType = responseType,
				ResponseMessage = responseMessage,
				TimelineStatus = timelineStatus,
				Media = media
			};
		}
	}
}
