using MoreLinq;
using Quarkless.Services.Interfaces;
using Quarkless.Services.Interfaces.Actions;
using Quarkless.Services.StrategyBuilders;
using QuarklessContexts.Classes.Carriers;
using QuarklessContexts.Enums;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.ServicesModels.HeartbeatModels;
using QuarklessContexts.Models.ServicesModels.SearchModels;
using QuarklessContexts.Models.Timeline;
using QuarklessLogic.Handlers.RequestBuilder.Consts;
using QuarklessLogic.ServicesLogic.HeartbeatLogic;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Quarkless.Services.ActionBuilders.EngageActions
{
	public enum LikeActionType
	{
		Any = 0,
		LikeByTopic = 1,
		LikeUsersMediaByLikers = 2,
		LikeFromUsersFeed = 4,
		LikeUsersMediaByCommenters = 5,
		LikeUsersMediaByLocation = 6
	}
	public class LikeMediaAction : IActionCommit
	{
		private readonly IContentManager _builder;
		private readonly IHeartbeatLogic _heartbeatLogic;
		private UserStoreDetails user;
		private LikeStrategySettings likeStrategySettings;
		public LikeMediaAction(IContentManager builder,IHeartbeatLogic heartbeatLogic)
		{
			_builder = builder;
			_heartbeatLogic = heartbeatLogic;
		}

		public IActionCommit IncludeStrategy(IStrategySettings strategy)
		{
			this.likeStrategySettings = strategy as LikeStrategySettings;
			return this;
		}
		private string LikeUserFeedMedia()
		{
			var by = new By
			{
				ActionType = (int)ActionType.LikePost,
				User = user.Profile.InstagramAccountId
			};
			var fetchMedias = _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchUsersFeed, user.Profile.Topics.TopicFriendlyName, user.Profile.InstagramAccountId)
				.GetAwaiter().GetResult().Where(exclude=>!exclude.SeenBy.Any(e=>e.User == by.User && e.ActionType == by.ActionType))
				.Where(s=>s.ObjectItem.Medias.Count>0);
			if (fetchMedias == null) return null;
			var meta_S = fetchMedias as __Meta__<Media>[] ?? fetchMedias.ToArray();
			var select = meta_S.ElementAtOrDefault(SecureRandom.Next(meta_S.Count()));
			if (@select == null) return null;
			@select.SeenBy.Add(@by);
			_heartbeatLogic.UpdateMetaData(MetaDataType.FetchUsersFeed, user.Profile.Topics.TopicFriendlyName, @select,user.Profile.InstagramAccountId).GetAwaiter().GetResult();
			return @select.ObjectItem.Medias.FirstOrDefault()?.MediaId;
		}
		private string LikeUsersMediaByTopic()
		{
			var by = new By
			{
				ActionType = (int)ActionType.LikePost,
				User = user.Profile.InstagramAccountId
			};
			var fetchMedias = _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaByTopic, user.Profile.Topics.TopicFriendlyName)
				.GetAwaiter().GetResult().Where(exclude=>!exclude.SeenBy.Any(e=>e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Medias.Count > 0);
			var meta_S = fetchMedias as __Meta__<Media>[] ?? fetchMedias.ToArray();
			var select = meta_S.ElementAtOrDefault(SecureRandom.Next(meta_S.Count()));
			if (@select == null) return null;
			@select.SeenBy.Add(@by);
			_heartbeatLogic.UpdateMetaData(MetaDataType.FetchMediaByTopic, user.Profile.Topics.TopicFriendlyName, @select).GetAwaiter().GetResult();
			return @select.ObjectItem.Medias.FirstOrDefault()?.MediaId;
		}
		private string LikeUsersMediaByLocation()
		{
			var by = new By
			{
				ActionType = (int)ActionType.LikePost,
				User = user.Profile.InstagramAccountId
			};
			var fetchMedias = _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaByUserLocationTargetList, user.Profile.Topics.TopicFriendlyName,user.Profile.InstagramAccountId)
				.GetAwaiter().GetResult().Where(exclude=>!exclude.SeenBy.Any(e=>e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Medias.Count > 0);
			if (fetchMedias == null) return null;
			var meta_S = fetchMedias as __Meta__<Media>[] ?? fetchMedias.ToArray();
			var select = meta_S.ElementAtOrDefault(SecureRandom.Next(meta_S.Count()));
			if (@select == null) return null;
			@select.SeenBy.Add(@by);
			_heartbeatLogic.UpdateMetaData(MetaDataType.FetchMediaByUserLocationTargetList, user.Profile.Topics.TopicFriendlyName, @select,user.Profile.InstagramAccountId).GetAwaiter().GetResult();
			return @select.ObjectItem.Medias.FirstOrDefault()?.MediaId;
		}
		private string LikeUsersMediaLikers()
		{
			var by = new By
			{
				ActionType = (int)ActionType.LikePost,
				User = user.Profile.InstagramAccountId
			};
			var fetchMedias = _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaByLikers, user.Profile.Topics.TopicFriendlyName)
				.GetAwaiter().GetResult().Where(exclude=>!exclude.SeenBy.Any(e=>e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Medias.Count > 0);
			if (fetchMedias == null) return null;
			var meta_S = fetchMedias as __Meta__<Media>[] ?? fetchMedias.ToArray();
			var select = meta_S.ElementAtOrDefault(SecureRandom.Next(meta_S.Count()));
			if (@select == null) return null;
			@select.SeenBy.Add(@by);
			_heartbeatLogic.UpdateMetaData(MetaDataType.FetchMediaByLikers, user.Profile.Topics.TopicFriendlyName, @select).GetAwaiter().GetResult();
			return @select.ObjectItem.Medias.FirstOrDefault()?.MediaId;
		}
		private string LikeUsersMediaCommenters()
		{
			var by = new By
			{
				ActionType = (int)ActionType.LikePost,
				User = user.Profile.InstagramAccountId
			};
			var fetchMedias = _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaByCommenters, user.Profile.Topics.TopicFriendlyName)
				.GetAwaiter().GetResult().Where(exclude=>!exclude.SeenBy.Any(e=>e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Medias.Count > 0);
			var meta_S = fetchMedias as __Meta__<Media>[] ?? fetchMedias.ToArray();
			var select = meta_S.ElementAtOrDefault(SecureRandom.Next(meta_S.Count()));
			if (@select == null) return null;
			@select.SeenBy.Add(@by);
			_heartbeatLogic.UpdateMetaData(MetaDataType.FetchMediaByCommenters, user.Profile.Topics.TopicFriendlyName, @select).GetAwaiter().GetResult();
			return @select.ObjectItem.Medias.FirstOrDefault()?.MediaId;
		}
		public ResultCarrier<IEnumerable<TimelineEventModel>> Push(IActionOptions actionOptions)
		{
			Console.WriteLine($"Like Post Action Started: {user.OAccountId}, {user.OInstagramAccountUsername}, {user.OInstagramAccountUser}");

			var results = new ResultCarrier<IEnumerable<TimelineEventModel>>();
			var likeActionOptions = actionOptions as LikeActionOptions;
			if (likeStrategySettings == null && user==null)
			{
				results.IsSuccesful = false;
				results.Info = new ErrorResponse
				{
					Message = $"user is null, user: {user.OAccountId}, instaId: {user.OInstagramAccountUsername}",
					StatusCode = System.Net.HttpStatusCode.NotFound
				};
				return results;
			};
			try
			{
				switch (likeStrategySettings.LikeStrategy)
				{
					case LikeStrategyType.Default:
					{
						var nominatedMedia = string.Empty;
						var likeActionTypeSelected = LikeActionType.LikeByTopic;
						if (likeActionOptions != null && likeActionOptions.LikeActionType == LikeActionType.Any)
						{
							var likeActionsChances = new List<Chance<LikeActionType>>
							{
								new Chance<LikeActionType>{Object = LikeActionType.LikeByTopic, Probability = 0.10},
								new Chance<LikeActionType>{Object = LikeActionType.LikeFromUsersFeed, Probability = 0.30},
								new Chance<LikeActionType>{Object = LikeActionType.LikeUsersMediaByCommenters, Probability = 0.15},
								new Chance<LikeActionType>{Object = LikeActionType.LikeUsersMediaByLikers, Probability = 0.29},
							};

							if (user.Profile.LocationTargetList != null)
								if(user.Profile.LocationTargetList.Count > 0)
									likeActionsChances.Add(new Chance<LikeActionType> { Object = LikeActionType.LikeUsersMediaByLocation, Probability = 0.16 });

							likeActionTypeSelected = SecureRandom.ProbabilityRoll(likeActionsChances);
						}
						else
						{
							likeActionTypeSelected = likeActionOptions.LikeActionType;
						}

						switch (likeActionTypeSelected)
						{
							case LikeActionType.LikeByTopic:
								nominatedMedia = LikeUsersMediaByTopic();
								break;
							case LikeActionType.LikeFromUsersFeed:
								nominatedMedia = LikeUserFeedMedia();
								break;
							case LikeActionType.LikeUsersMediaByCommenters:
								nominatedMedia = LikeUsersMediaCommenters();
								break;
							case LikeActionType.LikeUsersMediaByLikers:
								nominatedMedia = LikeUsersMediaLikers();
								break;
							case LikeActionType.LikeUsersMediaByLocation:
								nominatedMedia = LikeUsersMediaByLocation();
								break;
						}
						if (string.IsNullOrEmpty(nominatedMedia))
						{
							results.IsSuccesful = false;
							results.Info = new ErrorResponse
							{
								Message = $"could not find any good media to like, user: {user.OAccountId}, instaId: {user.OInstagramAccountUsername}",
								StatusCode = System.Net.HttpStatusCode.NotFound
							};
							return results;
						}

						var restModel = new RestModel
						{
							BaseUrl = string.Format(UrlConstants.LikeMedia, nominatedMedia),
							RequestType = RequestType.POST,
							User = user,
							JsonBody = null
						};
						results.IsSuccesful = true;
						results.Results = new List<TimelineEventModel>
						{
							new TimelineEventModel
							{
								ActionName = $"LikeMedia_{likeStrategySettings.LikeStrategy.ToString()}_{likeActionTypeSelected.ToString()}",
								Data = restModel,
								ExecutionTime = likeActionOptions.ExecutionTime
							}
						};
						return results;
					}
					case LikeStrategyType.TwoDollarCent:
						List<TimelineEventModel> events_ = new List<TimelineEventModel>();
					{
						var by = new By
						{
							ActionType = (int)ActionType.LikePost,
							User = user.Profile.InstagramAccountId
						};
						var fetchMedias = _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaByTopic, user.Profile.Topics.TopicFriendlyName)
							.GetAwaiter().GetResult().Where(exclude=>!exclude.SeenBy.Any(e=>e.User == @by.User && e.ActionType == @by.ActionType));
							var timerCounter = 0;
							var grouped = fetchMedias.GroupBy(a=>a.ObjectItem.Medias.FirstOrDefault()?.Topic);
							foreach(var topic in grouped)
							{
								for (var i = 0; i < likeStrategySettings.NumberOfActions; i++)
								{
									var media = topic.ElementAtOrDefault(i);
									if (media == null) continue;
									media.SeenBy.Add(@by);
									_heartbeatLogic.UpdateMetaData(MetaDataType.FetchUsersFeed,
										user.Profile.Topics.TopicFriendlyName, media,
										user.Profile.InstagramAccountId).GetAwaiter().GetResult();
									var nominatedMedia = media.ObjectItem.Medias.FirstOrDefault()?.MediaId;
									if (nominatedMedia == null) continue;
									var restModel = new RestModel
									{
										BaseUrl = string.Format(UrlConstants.LikeMedia, nominatedMedia),
										RequestType = RequestType.POST,
										JsonBody = null,
										User = user
									};
									events_.Add(new TimelineEventModel
									{
										ActionName =
											$"LikeMedia{likeStrategySettings.LikeStrategy.ToString()}_{likeActionOptions?.LikeActionType.ToString()}",
										Data = restModel,
										ExecutionTime = likeActionOptions.ExecutionTime.AddMinutes(
											(likeStrategySettings.OffsetPerAction.TotalMinutes) * timerCounter++)
									});
								}
							}
							results.IsSuccesful = true;
							results.Results = events_;
							return results;
					}
				}
				results.IsSuccesful = false;
				results.Info = new ErrorResponse
				{
					Message = $"strategy not implemented, user: {user.OAccountId}, instaId: {user.OInstagramAccountUsername}",
					StatusCode = System.Net.HttpStatusCode.Forbidden
				};
				return results;
			}
			catch (Exception ee)
			{
				results.IsSuccesful = false;
				results.Info = new ErrorResponse
				{
					Message = $"{ee.Message}, user: {user.OAccountId}, instaId: {user.OInstagramAccountUsername}",
					StatusCode = System.Net.HttpStatusCode.InternalServerError,
					Exception = ee
				};
				return results;
			}
		}

		public IActionCommit IncludeUser(UserStoreDetails userStoreDetails)
		{
			user = userStoreDetails;
			return this;
		}
	}
}
