﻿using MoreLinq;
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
		LikeUsersMediaByLikersInDepth = 3,
		LikeFromUsersFeed = 4,
		LikeUsersMediaByCommenters = 5
	}
	public class LikeMediaAction : IActionCommit
	{
		private readonly IContentManager _builder;
		private readonly IHeartbeatLogic _heartbeatLogic;
		private readonly ProfileModel _profile;
		private UserStoreDetails user;
		private LikeStrategySettings likeStrategySettings;
		public LikeMediaAction(IContentManager builder,IHeartbeatLogic heartbeatLogic,ProfileModel profile)
		{
			_builder = builder;
			_profile = profile;
			_heartbeatLogic = heartbeatLogic;
		}

		public IActionCommit IncludeStrategy(IStrategySettings strategy)
		{
			this.likeStrategySettings = strategy as LikeStrategySettings;
			return this;
		}
		private string LikeUserFeedMedia()
		{
			var fetchMedias = _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchUsersFeed, _profile.Topics.TopicFriendlyName, _profile.InstagramAccountId).GetAwaiter().GetResult();
			if (fetchMedias != null)
			{
				var select = fetchMedias.ElementAtOrDefault(SecureRandom.Next(fetchMedias.Count()));
				if (select.HasValue)
				{
					By by = new By
					{
						ActionType = (int) ActionType.LikePost,
						User = _profile.InstagramAccountId
					};
					if (!select.Value.SeenBy.Contains(by))
					{
						select.Value.SeenBy.Add(by);
						_heartbeatLogic.UpdateMetaData(MetaDataType.FetchUsersFeed, _profile.Topics.TopicFriendlyName, select.Value,_profile.InstagramAccountId).GetAwaiter().GetResult();
						return select.Value.ObjectItem.Medias.FirstOrDefault().MediaId;
					}
				}
			}
			return null;
		}
		private string LikeUsersMediaByTopic()
		{
			var fetchMedias = _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaByTopic, _profile.Topics.TopicFriendlyName).GetAwaiter().GetResult();
			if (fetchMedias != null)
			{
				var select = fetchMedias.ElementAtOrDefault(SecureRandom.Next(fetchMedias.Count()));
				if (select.HasValue)
				{
					By by = new By
					{
						ActionType = (int)ActionType.LikePost,
						User = _profile.InstagramAccountId
					};
					if (!select.Value.SeenBy.Contains(by))
					{
						select.Value.SeenBy.Add(by);
						_heartbeatLogic.UpdateMetaData(MetaDataType.FetchMediaByTopic, _profile.Topics.TopicFriendlyName, select.Value).GetAwaiter().GetResult();
						return select.Value.ObjectItem.Medias.FirstOrDefault().MediaId;
					}
				}
			}
			return null;
		}
		private string LikeUsersMediaLikers(bool inDepth = false)
		{
			var fetchMedias = _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaByLikers, _profile.Topics.TopicFriendlyName).GetAwaiter().GetResult();
			if (fetchMedias != null)
			{
				var select = fetchMedias.ElementAtOrDefault(SecureRandom.Next(fetchMedias.Count()));
				if (select.HasValue)
				{
					By by = new By
					{
						ActionType = (int)ActionType.LikePost,
						User = _profile.InstagramAccountId
					};
					if (!select.Value.SeenBy.Contains(by))
					{
						select.Value.SeenBy.Add(by);
						_heartbeatLogic.UpdateMetaData(MetaDataType.FetchMediaByLikers, _profile.Topics.TopicFriendlyName, select.Value).GetAwaiter().GetResult();
						return select.Value.ObjectItem.Medias.FirstOrDefault().MediaId;
					}
				}
			}
			return null;
		}
		private string LikeUsersMediaCommenters()
		{
			var fetchMedias = _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaByCommenters, _profile.Topics.TopicFriendlyName).GetAwaiter().GetResult();
			if (fetchMedias != null)
			{
				var select = fetchMedias.ElementAtOrDefault(SecureRandom.Next(fetchMedias.Count()));
				if (select.HasValue)
				{
					By by = new By
					{
						ActionType = (int)ActionType.LikePost,
						User = _profile.InstagramAccountId
					};
					if (!select.Value.SeenBy.Contains(by))
					{
						select.Value.SeenBy.Add(by);
						_heartbeatLogic.UpdateMetaData(MetaDataType.FetchMediaByCommenters, _profile.Topics.TopicFriendlyName, select.Value).GetAwaiter().GetResult();
						return select.Value.ObjectItem.Medias.FirstOrDefault().MediaId;
					}
				}
			}
			return null;
		}
		public ResultCarrier<IEnumerable<TimelineEventModel>> Push(IActionOptions actionOptions)
		{
			ResultCarrier<IEnumerable<TimelineEventModel>> Results = new ResultCarrier<IEnumerable<TimelineEventModel>>();
			LikeActionOptions likeActionOptions = actionOptions as LikeActionOptions;
			if (likeStrategySettings == null && user==null)
			{
				Results.IsSuccesful = false;
				Results.Info = new ErrorResponse
				{
					Message = $"user is null, user: {user.OAccountId}, instaId: {user.OInstagramAccountUsername}",
					StatusCode = System.Net.HttpStatusCode.NotFound
				};
				return Results;
			};
			try
			{
				Console.WriteLine("Like Action Started");
				if (likeStrategySettings.LikeStrategy == LikeStrategyType.Default)
				{
					string nominatedMedia = string.Empty;
					LikeActionType likeActionTypeSelected = LikeActionType.LikeByTopic;
					if (likeActionOptions.LikeActionType == LikeActionType.Any)
					{
						List<Chance<LikeActionType>> likeActionsChances = new List<Chance<LikeActionType>>
						{
							new Chance<LikeActionType>{Object = LikeActionType.LikeByTopic, Probability = 0.10},
							new Chance<LikeActionType>{Object = LikeActionType.LikeUsersMediaByLikersInDepth, Probability = 0.30},
							new Chance<LikeActionType>{Object = LikeActionType.LikeFromUsersFeed, Probability = 0.30},
							new Chance<LikeActionType>{Object = LikeActionType.LikeUsersMediaByCommenters, Probability = 0.15},
							new Chance<LikeActionType>{Object = LikeActionType.LikeUsersMediaByLikers, Probability = 0.15}
						};
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
							nominatedMedia = LikeUsersMediaLikers(false);
							break;
						case LikeActionType.LikeUsersMediaByLikersInDepth:
							nominatedMedia = LikeUsersMediaLikers(true);
							break;
					}
					if (string.IsNullOrEmpty(nominatedMedia))
					{
						Results.IsSuccesful = false;
						Results.Info = new ErrorResponse
						{
							Message = $"could not find any good media to like, user: {user.OAccountId}, instaId: {user.OInstagramAccountUsername}",
							StatusCode = System.Net.HttpStatusCode.NotFound
						};
						return Results;
					}
					RestModel restModel = new RestModel
					{
						BaseUrl = string.Format(UrlConstants.LikeMedia, nominatedMedia),
						RequestType = RequestType.POST,
						User = user,
						JsonBody = null
					};
					Results.IsSuccesful = true;
					Results.Results = new List<TimelineEventModel>
					{
						new TimelineEventModel
						{
							ActionName = $"LikeMedia_{likeStrategySettings.LikeStrategy.ToString()}_{likeActionTypeSelected.ToString()}",
							Data = restModel,
							ExecutionTime = likeActionOptions.ExecutionTime
						}
					};
					return Results;
				}
				else if(likeStrategySettings.LikeStrategy == LikeStrategyType.TwoDollarCent)
				{
					var fetchMedias = _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaByTopic, _profile.Topics.TopicFriendlyName).GetAwaiter().GetResult();
					if (fetchMedias != null)
					{
						int timerCounter = 0;
						List<TimelineEventModel> events_ = new List<TimelineEventModel>();
						var grouped = fetchMedias.Where(s=>s.HasValue).GroupBy(a=>a.Value.ObjectItem.Medias.FirstOrDefault().Topic);
						foreach(var topic in grouped)
						{
							for (int i = 0; i < likeStrategySettings.NumberOfActions; i++)
							{
								var media = topic.ElementAtOrDefault(i).Value;
								var by = new By
								{
									ActionType = (int)ActionType.LikePost,
									User = _profile.InstagramAccountId
								};
								if (!media.SeenBy.Contains(by))
								{
									media.SeenBy.Add(by);
									_heartbeatLogic.UpdateMetaData(MetaDataType.FetchUsersFeed, _profile.Topics.TopicFriendlyName, media, _profile.InstagramAccountId).GetAwaiter().GetResult();
									var nominatedMedia = media.ObjectItem.Medias.FirstOrDefault().MediaId;
									if (nominatedMedia != null)
									{
										RestModel restModel = new RestModel
										{
											BaseUrl = string.Format(UrlConstants.LikeMedia, nominatedMedia),
											RequestType = RequestType.POST,
											JsonBody = null,
											User = user
										};
										events_.Add(new TimelineEventModel
										{
											ActionName = $"LikeMedia{likeStrategySettings.LikeStrategy.ToString()}_{likeActionOptions.LikeActionType.ToString()}",
											Data = restModel,
											ExecutionTime = likeActionOptions.ExecutionTime.AddMinutes((likeStrategySettings.OffsetPerAction.TotalMinutes) * timerCounter++)
										});
									}
								}
								
							}
						}
						Results.IsSuccesful = true;
						Results.Results = events_;
						return Results;
					}
				}
				Results.IsSuccesful = false;
				Results.Info = new ErrorResponse
				{
					Message = $"strategy not implemented, user: {user.OAccountId}, instaId: {user.OInstagramAccountUsername}",
					StatusCode = System.Net.HttpStatusCode.Forbidden
				};
				return Results;
			}
			catch (Exception ee)
			{
				Results.IsSuccesful = false;
				Results.Info = new ErrorResponse
				{
					Message = $"{ee.Message}, user: {user.OAccountId}, instaId: {user.OInstagramAccountUsername}",
					StatusCode = System.Net.HttpStatusCode.InternalServerError,
					Exception = ee
				};
				return Results;
			}
		}

		public IActionCommit IncludeUser(UserStoreDetails userStoreDetails)
		{
			user = userStoreDetails;
			return this;
		}
	}
}
