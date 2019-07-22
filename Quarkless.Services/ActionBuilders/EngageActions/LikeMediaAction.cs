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
			By by = new By
			{
				ActionType = (int)ActionType.LikePost,
				User = _profile.InstagramAccountId
			};
			var fetchMedias = _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchUsersFeed, _profile.Topics.TopicFriendlyName, _profile.InstagramAccountId)
				.GetAwaiter().GetResult().Where(exclude=>!exclude.SeenBy.Any(e=>e.User == by.User && e.ActionType == by.ActionType))
				.Where(s=>s.ObjectItem.Medias.Count>0);
			if (fetchMedias != null)
			{
				var select = fetchMedias.ElementAtOrDefault(SecureRandom.Next(fetchMedias.Count()));
				if (select!=null)
				{
					select.SeenBy.Add(by);
					_heartbeatLogic.UpdateMetaData(MetaDataType.FetchUsersFeed, _profile.Topics.TopicFriendlyName, select,_profile.InstagramAccountId).GetAwaiter().GetResult();
					return select.ObjectItem.Medias.FirstOrDefault().MediaId;
				}
			}
			return null;
		}
		private string LikeUsersMediaByTopic()
		{
			By by = new By
			{
				ActionType = (int)ActionType.LikePost,
				User = _profile.InstagramAccountId
			};
			var fetchMedias = _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaByTopic, _profile.Topics.TopicFriendlyName)
				.GetAwaiter().GetResult().Where(exclude=>!exclude.SeenBy.Any(e=>e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Medias.Count > 0);
			if (fetchMedias != null)
			{
				var select = fetchMedias.ElementAtOrDefault(SecureRandom.Next(fetchMedias.Count()));
				if (select != null)
				{
					select.SeenBy.Add(by);
					_heartbeatLogic.UpdateMetaData(MetaDataType.FetchMediaByTopic, _profile.Topics.TopicFriendlyName, select).GetAwaiter().GetResult();
					return select.ObjectItem.Medias.FirstOrDefault().MediaId;
				}
			}
			return null;
		}
		private string LikeUsersMediaByLocation()
		{
			By by = new By
			{
				ActionType = (int)ActionType.LikePost,
				User = _profile.InstagramAccountId
			};
			var fetchMedias = _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaByUserLocationTargetList, _profile.Topics.TopicFriendlyName,_profile.InstagramAccountId)
				.GetAwaiter().GetResult().Where(exclude=>!exclude.SeenBy.Any(e=>e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Medias.Count > 0);
			if (fetchMedias != null)
			{
				var select = fetchMedias.ElementAtOrDefault(SecureRandom.Next(fetchMedias.Count()));
				if (select != null)
				{
					select.SeenBy.Add(by);
					_heartbeatLogic.UpdateMetaData(MetaDataType.FetchMediaByUserLocationTargetList, _profile.Topics.TopicFriendlyName, select,_profile.InstagramAccountId).GetAwaiter().GetResult();
					return select.ObjectItem.Medias.FirstOrDefault().MediaId;
				}
			}
			return null;
		}
		private string LikeUsersMediaLikers()
		{
			By by = new By
			{
				ActionType = (int)ActionType.LikePost,
				User = _profile.InstagramAccountId
			};
			var fetchMedias = _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaByLikers, _profile.Topics.TopicFriendlyName)
				.GetAwaiter().GetResult().Where(exclude=>!exclude.SeenBy.Any(e=>e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Medias.Count > 0);
			if (fetchMedias != null)
			{
				var select = fetchMedias.ElementAtOrDefault(SecureRandom.Next(fetchMedias.Count()));
				if (select != null)
				{
					select.SeenBy.Add(by);
					_heartbeatLogic.UpdateMetaData(MetaDataType.FetchMediaByLikers, _profile.Topics.TopicFriendlyName, select).GetAwaiter().GetResult();
					return select.ObjectItem.Medias.FirstOrDefault().MediaId;
				}
			}
			return null;
		}
		private string LikeUsersMediaCommenters()
		{
			By by = new By
			{
				ActionType = (int)ActionType.LikePost,
				User = _profile.InstagramAccountId
			};
			var fetchMedias = _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaByCommenters, _profile.Topics.TopicFriendlyName)
				.GetAwaiter().GetResult().Where(exclude=>!exclude.SeenBy.Any(e=>e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Medias.Count > 0);
			if (fetchMedias != null)
			{
				var select = fetchMedias.ElementAtOrDefault(SecureRandom.Next(fetchMedias.Count()));
				if (select != null)
				{
						select.SeenBy.Add(by);
						_heartbeatLogic.UpdateMetaData(MetaDataType.FetchMediaByCommenters, _profile.Topics.TopicFriendlyName, select).GetAwaiter().GetResult();
						return select.ObjectItem.Medias.FirstOrDefault().MediaId;
				}
			}
			return null;
		}
		public ResultCarrier<IEnumerable<TimelineEventModel>> Push(IActionOptions actionOptions)
		{
			Console.WriteLine("Like Post Action Started");

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
				if (likeStrategySettings.LikeStrategy == LikeStrategyType.Default)
				{
					string nominatedMedia = string.Empty;
					LikeActionType likeActionTypeSelected = LikeActionType.LikeByTopic;
					if (likeActionOptions.LikeActionType == LikeActionType.Any)
					{
						List<Chance<LikeActionType>> likeActionsChances = new List<Chance<LikeActionType>>
						{
							new Chance<LikeActionType>{Object = LikeActionType.LikeByTopic, Probability = 0.10},
							new Chance<LikeActionType>{Object = LikeActionType.LikeFromUsersFeed, Probability = 0.30},
							new Chance<LikeActionType>{Object = LikeActionType.LikeUsersMediaByCommenters, Probability = 0.15},
							new Chance<LikeActionType>{Object = LikeActionType.LikeUsersMediaByLikers, Probability = 0.29},
						};

						if (_profile.LocationTargetList != null)
							if(_profile.LocationTargetList.Count > 0)
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
						Results.IsSuccesful = false;
						Results.Info = new ErrorResponse
						{
							Message = $"could not find any good media to like, user: {user.OAccountId}, instaId: {user.OInstagramAccountUsername}",
							StatusCode = System.Net.HttpStatusCode.NotFound
						};
						return Results;
					}
					else { 
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
				}
				else if(likeStrategySettings.LikeStrategy == LikeStrategyType.TwoDollarCent)
				{
					By by = new By
					{
						ActionType = (int)ActionType.LikePost,
						User = _profile.InstagramAccountId
					};
					var fetchMedias = _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaByTopic, _profile.Topics.TopicFriendlyName)
						.GetAwaiter().GetResult().Where(exclude=>!exclude.SeenBy.Any(e=>e.User == by.User && e.ActionType == by.ActionType));
					if (fetchMedias != null)
					{
						int timerCounter = 0;
						List<TimelineEventModel> events_ = new List<TimelineEventModel>();
						var grouped = fetchMedias.Where(s=>s!=null).GroupBy(a=>a.ObjectItem.Medias.FirstOrDefault().Topic);
						foreach(var topic in grouped)
						{
							for (int i = 0; i < likeStrategySettings.NumberOfActions; i++)
							{
								var media = topic.ElementAtOrDefault(i);
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
