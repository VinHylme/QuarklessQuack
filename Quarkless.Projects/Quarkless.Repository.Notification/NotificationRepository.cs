using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Notification;
using Quarkless.Models.Notification.Enums;
using Quarkless.Models.Notification.Interfaces;
using Quarkless.Repository.MongoContext;

namespace Quarkless.Repository.Notification
{
	public class NotificationRepository : INotificationRepository
	{
		private readonly IMongoCollection<NotificationTimelineAction> _ctx;
		private readonly IMongoCollection<NotificationObject> _ctxNotify;
		private const string NOTIFICATION_TIMELINE_ACTION_COLLECTION_NAME = "TimelineNotifications";
		private const string NOTIFICATION_COLLECTION_NAME = "Notifications";
		public NotificationRepository(IMongoClientContext context)
		{ 
			_ctx = context.AccountDatabase.GetCollection<NotificationTimelineAction>(NOTIFICATION_TIMELINE_ACTION_COLLECTION_NAME);
			_ctxNotify = context.AccountDatabase.GetCollection<NotificationObject>(NOTIFICATION_COLLECTION_NAME);
		}

		public async Task AddNotification(NotificationObject notificationItem)
		{
			try
			{
				await _ctxNotify.InsertOneAsync(notificationItem);
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
			}
		}

		public async Task AddTimelineActionNotification(NotificationTimelineAction notificationItem)
		{
			try
			{
				var uniqueId = ObjectId.GenerateNewId(DateTime.UtcNow).ToString();
				notificationItem._id = uniqueId;
				notificationItem.Notification._id = uniqueId;
				await _ctx.InsertOneAsync(notificationItem);
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
			}
		}

		public async Task MarkNotificationAsRead(params string[] notificationIds)
		{
			try
			{
				await _ctx.UpdateManyAsync(Builders<NotificationTimelineAction>
						.Filter.In(_ => _.Notification._id, notificationIds),
					Builders<NotificationTimelineAction>.Update
						.Set(_=>_.Notification.Status, NotificationStatus.Seen)
						.Set(_=>_.Notification.SeenAt, DateTime.UtcNow));
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
			}
		}

		public async Task DeleteNotifications(params string[] notificationIds)
		{
			try
			{
				await _ctx.DeleteManyAsync(Builders<NotificationTimelineAction>
					.Filter.In(_ => _.Notification._id, notificationIds));
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
			}
		}

		public async Task<List<NotificationTimelineAction>> GetTimelineActionNotificationByResponseType(
			string accountId, string instagramAccountId, int limit = 150, params int[] types)
		{
			try
			{
				var findOptions = new FindOptions<NotificationTimelineAction, NotificationTimelineAction>
				{
					Limit = limit,
					Sort = Builders<NotificationTimelineAction>.Sort.Descending(_ => _.Notification.CreatedAt)
				};

				var filterBuilder = new FilterDefinitionBuilder<NotificationTimelineAction>();
				var filter = filterBuilder.In(_ => _.ResponseType, types) 
					& filterBuilder.Eq(s => s.Notification.AccountId, accountId)
					& filterBuilder.Eq(_ => _.Notification.InstagramAccountId, instagramAccountId)
					& filterBuilder.Eq(_=>_.Notification.Status, NotificationStatus.Pending);

				var results = await _ctx.FindAsync(filter, findOptions);
				return results.ToList();
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
				return new List<NotificationTimelineAction>();
			}
		}

		public async Task<int> OccurrencesByResponseType(string accountId, string instagramAccountId,
			int limit = 150, params int[] types)
		{
			try
			{
				var countOptions = new CountOptions
				{
					Limit = limit
				};

				var filterBuilder = new FilterDefinitionBuilder<NotificationTimelineAction>();
				var filter = filterBuilder.In(_ => _.ResponseType, types)
							& filterBuilder.Eq(s => s.Notification.AccountId, accountId)
							& filterBuilder.Eq(_ => _.Notification.InstagramAccountId, instagramAccountId);

				var results = await _ctx.CountDocumentsAsync(filter, countOptions);
				return (int) results;
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
				return 0;
			}
		}

		public async Task<List<NotificationTimelineAction>> GetMarkedTimelineActionNotifications(int limit = 400)
		{
			try
			{
				var findOptions = new FindOptions<NotificationTimelineAction, NotificationTimelineAction>
				{
					Limit = limit,
					Sort = Builders<NotificationTimelineAction>.Sort.Descending(_ => _.Notification.CreatedAt)
				};

				return (await _ctx.FindAsync(_ => _.Notification.Status == NotificationStatus.Seen, findOptions))
					.ToList();
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
				return new List<NotificationTimelineAction>();
			}
		}

		public async Task<List<NotificationTimelineAction>> GetTimelineActionNotifications(int limit = 250,
			ActionType actionType = ActionType.All, TimelineEventItemStatus? status = null)
		{
			try
			{
				var findOptions = new FindOptions<NotificationTimelineAction, NotificationTimelineAction>
				{
					Limit = limit,
					Sort = Builders<NotificationTimelineAction>.Sort.Descending(_ => _.Notification.CreatedAt)
				};

				var results = status switch
				{
					null when actionType == ActionType.All 
					=> (await _ctx.FindAsync(_ => _.Notification.Status == NotificationStatus.Pending, findOptions)).ToList(),
					null when actionType != ActionType.All 
					=> (await _ctx.FindAsync(_ => _.Notification.Status == NotificationStatus.Pending && _.ActionType == actionType, findOptions)).ToList(),
					{} when actionType == ActionType.All 
					=> (await _ctx.FindAsync(_ => _.Notification.Status == NotificationStatus.Pending && _.TimelineStatus == status.Value,
						findOptions)).ToList(),
					{} when actionType != ActionType.All 
					=> (await _ctx.FindAsync(_ => _.Notification.Status == NotificationStatus.Pending && _.TimelineStatus == status.Value && _.ActionType == actionType)).ToList(),
					_ => new List<NotificationTimelineAction>()
				};
				return results;
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
				return new List<NotificationTimelineAction>();
			}
		}

		public async Task<List<NotificationTimelineAction>> GetTimelineActionNotifications(string accountId, string instagramAccountId, int limit = 250,
			ActionType actionType = ActionType.All, TimelineEventItemStatus? status = null, bool includeSeen = false)
		{
			try
			{
				var findOptions = new FindOptions<NotificationTimelineAction, NotificationTimelineAction>
				{
					Limit = limit,
					Sort = Builders<NotificationTimelineAction>.Sort.Descending(_ => _.Notification.CreatedAt)
				};

				var filterBuilder = Builders<NotificationTimelineAction>.Filter;

				var filter = filterBuilder.Eq(_ => _.Notification.AccountId, accountId) &
							 filterBuilder.Eq(_ => _.Notification.InstagramAccountId, instagramAccountId);

				if (!includeSeen)
					filter &= filterBuilder.Eq(_ => _.Notification.Status, NotificationStatus.Pending);

				if (status.HasValue)
					filter &= filterBuilder.Eq(_ => _.TimelineStatus, status.Value);

				if (actionType != ActionType.All)
					filter &= filterBuilder.Eq(_ => _.ActionType, actionType);

				return (await _ctx.FindAsync(filter, findOptions)).ToList();

				/* mainly did this shit because wanted to try out the switch pattern matching
				var results = status switch
				{
					null when actionType == ActionType.All && !includeSeen => (await _ctx.FindAsync(_ =>
						_.Notification.Status == NotificationStatus.Pending  && _.Notification.AccountId == accountId 
						&& _.Notification.InstagramAccountId == instagramAccountId, findOptions)).ToList(),

					null when actionType != ActionType.All && !includeSeen => (await _ctx.FindAsync(_ =>
						_.Notification.Status == NotificationStatus.Pending && _.Notification.AccountId == accountId
						&& _.Notification.InstagramAccountId == instagramAccountId &&  _.ActionType == actionType,
						findOptions)).ToList(),

					{ } when actionType == ActionType.All && !includeSeen => (await _ctx.FindAsync(_ =>
						_.Notification.Status == NotificationStatus.Pending && _.Notification.AccountId == accountId
						&& _.Notification.InstagramAccountId == instagramAccountId && _.TimelineStatus == status.Value,
						findOptions)).ToList(),

					{ } when actionType != ActionType.All && !includeSeen => (await _ctx.FindAsync(_ =>
						_.Notification.Status == NotificationStatus.Pending &&
						_.Notification.AccountId == accountId && _.Notification.InstagramAccountId == instagramAccountId &&
						_.TimelineStatus == status.Value && _.ActionType == actionType)).ToList(),
					
					null when actionType == ActionType.All && includeSeen => (await _ctx.FindAsync(_ =>
						_.Notification.AccountId == accountId
						&& _.Notification.InstagramAccountId == instagramAccountId, findOptions)).ToList(),
					null when actionType != ActionType.All && includeSeen => (await _ctx.FindAsync(_ =>
							_.Notification.AccountId == accountId
						&& _.Notification.InstagramAccountId == instagramAccountId && _.ActionType == actionType,
						findOptions)).ToList(),

					{ } when actionType == ActionType.All && includeSeen => (await _ctx.FindAsync(_ =>
							_.Notification.AccountId == accountId
						&& _.Notification.InstagramAccountId == instagramAccountId && _.TimelineStatus == status.Value,
						findOptions)).ToList(),

					{ } when actionType != ActionType.All && includeSeen => (await _ctx.FindAsync(_ =>
						_.Notification.AccountId == accountId && _.Notification.InstagramAccountId == instagramAccountId &&
						_.TimelineStatus == status.Value && _.ActionType == actionType)).ToList(),
					_ => new List<NotificationTimelineAction>()
				};
				return results;*/
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
				return new List<NotificationTimelineAction>();
			}
		}

		public async Task DeleteNotifications(ActionType actionType = ActionType.All)
		{
			try
			{
				await _ctx.DeleteManyAsync(_=>_.ActionType == actionType);
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
			}
		}
	}
}