﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Notification.Enums;

namespace Quarkless.Models.Notification.Interfaces
{
	public interface INotificationLogic
	{
		public Task AddNotification(NotificationObject notificationItem);

		public Task AddTimelineActionNotification(NotificationTimelineAction notificationItem);

		public Task MarkNotificationAsRead(params string[] notificationIds);

		public Task DeleteNotifications(ActionType actionType = ActionType.All);

		public Task DeleteNotifications(params string[] notificationIds);
		Task<List<NotificationTimelineAction>> GetMarkedTimelineActionNotifications(int limit = 400);

		public Task<List<NotificationTimelineAction>> GetTimelineActionNotifications(string accountId,
			string instagramAccountId, int limit = 250, ActionType actionType = ActionType.All, 
			TimelineEventItemStatus? status = null, bool includeSeen = false);

		//gets for all users (only be used by admin)
		public Task<List<NotificationTimelineAction>> GetTimelineActionNotifications(int limit = 250, 
			ActionType actionType = ActionType.All, TimelineEventItemStatus? status = null);

		Task<List<NotificationTimelineAction>> GetTimelineActionNotificationByResponseType(string accountId,
			string instagramAccountId, int limit = 150, params int[] types);

		public Task<int> OccurrencesByResponseType(string accountId, string instagramAccountId,
			int limit = 150, params int[] types);
	}
}
