using System;
using System.Collections.Generic;
using System.Linq;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Common.Extensions;
using Quarkless.Models.Common.Models;
using Quarkless.Models.Common.Models.Carriers;
using Quarkless.Models.ContentInfo.Interfaces;
using Quarkless.Models.Heartbeat;
using Quarkless.Models.Heartbeat.Interfaces;
using Quarkless.Models.RequestBuilder.Interfaces;
using Quarkless.Models.SearchResponse;
using Quarkless.Models.Services.Automation.Enums.Actions.ActionType;
using Quarkless.Models.Services.Automation.Enums.Actions.StrategyType;
using Quarkless.Models.Services.Automation.Interfaces;
using Quarkless.Models.Services.Automation.Models.ActionOptions;
using Quarkless.Models.Services.Automation.Models.StrategySettings;
using Quarkless.Models.Storage.Interfaces;
using Quarkless.Models.Timeline;

namespace Quarkless.Logic.Services.Automation.Actions.EngageActions
{
	public class LikeMediaAction : IActionCommit
	{
		private readonly IContentInfoBuilder _builder;
		private readonly IHeartbeatLogic _heartbeatLogic;
		private readonly IUrlReader _urlReader;
		private UserStoreDetails user;
		private LikeStrategySettings likeStrategySettings;
		public LikeMediaAction(IContentInfoBuilder builder,IHeartbeatLogic heartbeatLogic, IUrlReader urlReader)
		{
			_builder = builder;
			_heartbeatLogic = heartbeatLogic;
			_urlReader = urlReader;
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
			var fetchMedias = _heartbeatLogic.GetMetaData<Media>(
					new MetaDataFetchRequest
					{
						MetaDataType = MetaDataType.FetchUsersFeed, 
						ProfileCategoryTopicId = user.Profile.ProfileTopic.Category._id,
						InstagramId = user.ShortInstagram.Id
					})
				.GetAwaiter().GetResult().Where(exclude=>!exclude.SeenBy.Any(e=>e.User == by.User && e.ActionType == by.ActionType))
				.Where(s=>s.ObjectItem.Medias.Count>0);

			if (fetchMedias == null) return null;
			var metaS = fetchMedias as Meta<Media>[] ?? fetchMedias.ToArray();
			var select = metaS.ElementAtOrDefault(SecureRandom.Next(metaS.Count()));
			if (select == null) return null;
			select.SeenBy.Add(by);
			_heartbeatLogic.UpdateMetaData(new MetaDataCommitRequest<Media>
			{
				MetaDataType = MetaDataType.FetchUsersFeed,
				ProfileCategoryTopicId = user.Profile.ProfileTopic.Category._id,
				InstagramId = user.ShortInstagram.Id,
				Data = select
			}).GetAwaiter().GetResult();
			return select.ObjectItem.Medias.FirstOrDefault()?.MediaId;
		}
		private string LikeUsersMediaByTopic()
		{
			var by = new By
			{
				ActionType = (int)ActionType.LikePost,
				User = user.Profile.InstagramAccountId
			};
			var fetchMedias = _heartbeatLogic.GetMetaData<Media>(new MetaDataFetchRequest
				{
					MetaDataType = MetaDataType.FetchMediaByTopic, 
					ProfileCategoryTopicId = user.Profile.ProfileTopic.Category._id,
					InstagramId = user.ShortInstagram.Id
				})
				.GetAwaiter().GetResult().Where(exclude=>!exclude.SeenBy.Any(e=>e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Medias.Count > 0);
			var metaS = fetchMedias as Meta<Media>[] ?? fetchMedias.ToArray();
			var select = metaS.ElementAtOrDefault(SecureRandom.Next(metaS.Count()));
			if (select == null) return null;
			select.SeenBy.Add(by);
			_heartbeatLogic.UpdateMetaData(new MetaDataCommitRequest<Media>
			{
				MetaDataType = MetaDataType.FetchMediaByTopic,
				ProfileCategoryTopicId = user.Profile.ProfileTopic.Category._id,
				InstagramId = user.ShortInstagram.Id,
				Data = select
			}).GetAwaiter().GetResult();
			return select.ObjectItem.Medias.FirstOrDefault()?.MediaId;
		}
		private string LikeUsersMediaByLocation()
		{
			var by = new By
			{
				ActionType = (int)ActionType.LikePost,
				User = user.Profile.InstagramAccountId
			};
			var fetchMedias = _heartbeatLogic.GetMetaData<Media>(new MetaDataFetchRequest
				{
					MetaDataType = MetaDataType.FetchMediaByUserLocationTargetList,
					ProfileCategoryTopicId = user.Profile.ProfileTopic.Category._id,
					InstagramId = user.ShortInstagram.Id
				})
				.GetAwaiter().GetResult().Where(exclude=>!exclude.SeenBy.Any(e=>e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Medias.Count > 0);
			if (fetchMedias == null) return null;
			var metaS = fetchMedias as Meta<Media>[] ?? fetchMedias.ToArray();
			var select = metaS.ElementAtOrDefault(SecureRandom.Next(metaS.Count()));
			if (select == null) return null;
			select.SeenBy.Add(by);
			_heartbeatLogic.UpdateMetaData(new MetaDataCommitRequest<Media>
			{
				MetaDataType = MetaDataType.FetchMediaByUserLocationTargetList,
				ProfileCategoryTopicId = user.Profile.ProfileTopic.Category._id,
				InstagramId = user.ShortInstagram.Id,
				Data = select
			}).GetAwaiter().GetResult();
			return select.ObjectItem.Medias.FirstOrDefault()?.MediaId;
		}
		private string LikeUsersMediaLikers()
		{
			var by = new By
			{
				ActionType = (int)ActionType.LikePost,
				User = user.Profile.InstagramAccountId
			};
			var fetchMedias = _heartbeatLogic.GetMetaData<Media>(
					new MetaDataFetchRequest
					{
						MetaDataType = MetaDataType.FetchMediaByLikers,
						ProfileCategoryTopicId = user.Profile.ProfileTopic.Category._id,
						InstagramId = user.ShortInstagram.Id
					})
				.GetAwaiter().GetResult().Where(exclude=>!exclude.SeenBy.Any(e=>e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Medias.Count > 0);
			if (fetchMedias == null) return null;
			var metaS = fetchMedias as Meta<Media>[] ?? fetchMedias.ToArray();
			var select = metaS.ElementAtOrDefault(SecureRandom.Next(metaS.Count()));
			if (select == null) return null;
			select.SeenBy.Add(by);
			_heartbeatLogic.UpdateMetaData(new MetaDataCommitRequest<Media>
			{
				MetaDataType = MetaDataType.FetchMediaByLikers,
				ProfileCategoryTopicId = user.Profile.ProfileTopic.Category._id,
				InstagramId = user.ShortInstagram.Id,
				Data = select
			}).GetAwaiter().GetResult();
			return select.ObjectItem.Medias.FirstOrDefault()?.MediaId;
		}
		private string LikeUsersMediaCommenters()
		{
			var by = new By
			{
				ActionType = (int)ActionType.LikePost,
				User = user.Profile.InstagramAccountId
			};
			var fetchMedias = _heartbeatLogic.GetMetaData<Media>(new MetaDataFetchRequest
				{
					MetaDataType = MetaDataType.FetchMediaByCommenters,
					ProfileCategoryTopicId = user.Profile.ProfileTopic.Category._id,
					InstagramId = user.ShortInstagram.Id
				})
				.GetAwaiter().GetResult().Where(exclude=>!exclude.SeenBy.Any(e=>e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Medias.Count > 0);
			var metaS = fetchMedias as Meta<Media>[] ?? fetchMedias.ToArray();
			var select = metaS.ElementAtOrDefault(SecureRandom.Next(metaS.Count()));
			if (select == null) return null;
			select.SeenBy.Add(by);
			_heartbeatLogic.UpdateMetaData(new MetaDataCommitRequest<Media>
			{
				MetaDataType = MetaDataType.FetchMediaByCommenters,
				ProfileCategoryTopicId = user.Profile.ProfileTopic.Category._id,
				InstagramId = user.ShortInstagram.Id,
				Data = select
			}).GetAwaiter().GetResult();
			return select.ObjectItem.Medias.FirstOrDefault()?.MediaId;
		}
		public ResultCarrier<IEnumerable<TimelineEventModel>> Push(IActionOptions actionOptions)
		{
			Console.WriteLine($"Like Post Action Started: {user.OAccountId}, {user.OInstagramAccountUsername}, {user.OInstagramAccountUser}");

			var results = new ResultCarrier<IEnumerable<TimelineEventModel>>();
			var likeActionOptions = actionOptions as LikeActionOptions;
			if (likeStrategySettings == null && user==null)
			{
				results.IsSuccessful = false;
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
							var likeActionsChances = new List<Chance<LikeActionType>>();

							if (user.Profile.LocationTargetList != null)
							{
								if (user.Profile.LocationTargetList.Count > 0)
								{
									likeActionsChances.AddRange(new List<Chance<LikeActionType>>
									{
										new Chance<LikeActionType>
										{
											Object = LikeActionType.LikeByTopic, 
											Probability = 0.10
										},
										new Chance<LikeActionType>
										{
											Object = LikeActionType.LikeFromUsersFeed, 
											Probability = user.Profile.AdditionalConfigurations.FocusLocalMore ? 0.15 : 0.30
										},
										new Chance<LikeActionType>
										{
											Object = LikeActionType.LikeUsersMediaByCommenters, 
											Probability = 0.15
										},
										new Chance<LikeActionType>
										{
											Object = LikeActionType.LikeUsersMediaByLikers, 
											Probability = 0.30
										},
										new Chance<LikeActionType>
										{
											Object = LikeActionType.LikeUsersMediaByLocation, 
											Probability = user.Profile.AdditionalConfigurations.FocusLocalMore ? 0.30 : 0.15
										}
									});
								}
								else
								{
									likeActionsChances.AddRange(new List<Chance<LikeActionType>>
									{
										new Chance<LikeActionType>
										{
											Object = LikeActionType.LikeByTopic, 
											Probability = 0.20
										},
										new Chance<LikeActionType>
										{
											Object = LikeActionType.LikeFromUsersFeed, 
											Probability = 0.30
										},
										new Chance<LikeActionType>
										{
											Object = LikeActionType.LikeUsersMediaByCommenters, 
											Probability = 0.20
										},
										new Chance<LikeActionType>
										{
											Object = LikeActionType.LikeUsersMediaByLikers, 
											Probability = 0.30
										},
									});
								}
							}

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
							results.IsSuccessful = false;
							results.Info = new ErrorResponse
							{
								Message = $"could not find any good media to like, user: {user.OAccountId}, instaId: {user.OInstagramAccountUsername}",
								StatusCode = System.Net.HttpStatusCode.NotFound
							};
							return results;
						}

						var restModel = new RestModel
						{
							BaseUrl = string.Format(_urlReader.LikeMedia, nominatedMedia),
							RequestType = RequestType.Post,
							User = user,
							JsonBody = null
						};
						results.IsSuccessful = true;
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
						var fetchMedias = _heartbeatLogic.GetMetaData<Media>(new MetaDataFetchRequest
							{
								MetaDataType = MetaDataType.FetchMediaByTopic,
								ProfileCategoryTopicId = user.Profile.ProfileTopic.Category._id,
								InstagramId = user.ShortInstagram.Id
							})
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
									_heartbeatLogic.UpdateMetaData(new MetaDataCommitRequest<Media>
									{
										MetaDataType = MetaDataType.FetchUsersFeed,
										ProfileCategoryTopicId = user.Profile.ProfileTopic.Category._id,
										InstagramId = user.ShortInstagram.Id,
										Data = media,
									}).GetAwaiter().GetResult();
									var nominatedMedia = media.ObjectItem.Medias.FirstOrDefault()?.MediaId;
									if (nominatedMedia == null) continue;
									var restModel = new RestModel
									{
										BaseUrl = string.Format(_urlReader.LikeMedia, nominatedMedia),
										RequestType = RequestType.Post,
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
							results.IsSuccessful = true;
							results.Results = events_;
							return results;
					}
				}
				results.IsSuccessful = false;
				results.Info = new ErrorResponse
				{
					Message = $"strategy not implemented, user: {user.OAccountId}, instaId: {user.OInstagramAccountUsername}",
					StatusCode = System.Net.HttpStatusCode.Forbidden
				};
				return results;
			}
			catch (Exception ee)
			{
				results.IsSuccessful = false;
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

		public IActionCommit IncludeStorage(IStorage storage)
		{
			throw new NotImplementedException();
		}
	}
}
