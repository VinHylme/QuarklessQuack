using System.Collections.Generic;
using System.Threading.Tasks;
using Quarkless.Base.Notification.Models;
using Quarkless.Base.Notification.Models.Enums;
using Quarkless.Base.Notification.Models.Interfaces;
using Quarkless.Models.Common.Enums;

namespace Quarkless.Base.Notification.Logic
{
	public class NotificationLogic : INotificationLogic
	{
		private readonly INotificationRepository _notificationRepository;
		public NotificationLogic(INotificationRepository notificationRepository)
		{
			_notificationRepository = notificationRepository;
		}

		public async Task AddNotification(NotificationObject notificationItem)
		{
			await _notificationRepository.AddNotification(notificationItem);
		}

		public async Task AddTimelineActionNotification(NotificationTimelineAction notificationItem)
		{
			await _notificationRepository.AddTimelineActionNotification(notificationItem);
		}

		public async Task MarkNotificationAsRead(params string[] notificationIds)
		{
			await _notificationRepository.MarkNotificationAsRead(notificationIds);
		}

		public async Task DeleteNotifications(params string[] notificationIds)
		{
			await _notificationRepository.DeleteNotifications(notificationIds);
		}

		public async Task<List<NotificationTimelineAction>> GetMarkedTimelineActionNotifications(int limit = 400)
		{
			return await _notificationRepository.GetMarkedTimelineActionNotifications(limit);
		}

		public async Task DeleteNotifications(ActionType actionType = ActionType.All)
		{
			await _notificationRepository.DeleteNotifications(actionType);
		}

		public async Task<List<NotificationTimelineAction>> GetTimelineActionNotificationByResponseType(string accountId, string instagramAccountId, int limit = 150,
			params int[] types)
		{
			return await _notificationRepository.GetTimelineActionNotificationByResponseType(accountId, instagramAccountId,
				limit, types);
		}

		public async Task<int> OccurrencesByResponseType(string accountId, string instagramAccountId,
			int limit = 150, params int[] types)
		{
			return await _notificationRepository.OccurrencesByResponseType(accountId, instagramAccountId, limit, types);
		}

		public async Task<List<NotificationTimelineAction>> GetTimelineActionNotifications(int limit = 250,
			ActionType actionType = ActionType.All, TimelineEventItemStatus? status = null)
		{
			return await _notificationRepository.GetTimelineActionNotifications(limit, actionType, status);
		}

		public async Task<List<NotificationTimelineAction>> GetTimelineActionNotifications(string accountId, string instagramAccountId, int limit = 250,
			ActionType actionType = ActionType.All, TimelineEventItemStatus? status = null, bool includeSeen = false)
		{
			return await _notificationRepository.GetTimelineActionNotifications(accountId, instagramAccountId, limit,
				actionType, status, includeSeen);
		}
	}
}
