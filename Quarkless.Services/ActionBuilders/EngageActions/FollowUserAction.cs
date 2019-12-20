using Quarkless.Services.Interfaces;
using Quarkless.Services.Interfaces.Actions;
using Quarkless.Services.StrategyBuilders;
using QuarklessContexts.Classes.Carriers;
using QuarklessContexts.Enums;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models;
using QuarklessContexts.Models.ServicesModels.HeartbeatModels;
using QuarklessContexts.Models.ServicesModels.SearchModels;
using QuarklessContexts.Models.Timeline;
using QuarklessLogic.Handlers.RequestBuilder.Constants;
using QuarklessLogic.ServicesLogic.HeartbeatLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using QuarklessLogic.Handlers.ContentInfoBuilder;
using QuarklessLogic.Logic.StorageLogic;

namespace Quarkless.Services.ActionBuilders.EngageActions
{
	public enum FollowActionType
	{
		Any = 0,
		FollowBasedOnLikers = 1,
		FollowBasedOnCommenters = 2,
		FollowBasedOnTopic = 3,
		FollowBasedOnLocation = 4,
		FollowBasedOnSuggestions = 5
	}
	public class FollowUserAction : IActionCommit
	{
		private readonly IContentInfoBuilder _builder;
		private readonly IHeartbeatLogic _heartbeatLogic;
		private readonly IUrlReader _urlReader;
		private UserStoreDetails user;
		private FollowStrategySettings followStrategySettings;
		public FollowUserAction(IContentInfoBuilder builder, IHeartbeatLogic heartbeatLogic, IUrlReader urlReader)
		{
			_builder = builder;
			_heartbeatLogic = heartbeatLogic;
			_urlReader = urlReader;
		}
		public IActionCommit IncludeStrategy(IStrategySettings strategy)
		{
			this.followStrategySettings = strategy as FollowStrategySettings;
			return this;
		}
		private long FollowBasedOnLikers()
		{
			var by = new By
			{
				ActionType = (int)ActionType.FollowUser,
				User = user.Profile.InstagramAccountId
			};
			var fetchMedias = _heartbeatLogic.GetMetaData<List<UserResponse<string>>>(MetaDataType.FetchUsersViaPostLiked, user.Profile.ProfileTopic.Category._id)
				.GetAwaiter().GetResult().Where(exclude=>!exclude.SeenBy.Any(e=>e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Count > 0);
			var meta_S = fetchMedias as __Meta__<List<UserResponse<string>>>[] ?? fetchMedias.ToArray();
			var select = meta_S.ElementAtOrDefault(SecureRandom.Next(meta_S.Count()));
			if (@select == null) return 0;
			@select.SeenBy.Add(@by);
			_heartbeatLogic.UpdateMetaData(MetaDataType.FetchUsersViaPostLiked, user.Profile.ProfileTopic.Category._id, @select).GetAwaiter().GetResult();
			return @select.ObjectItem?.ElementAtOrDefault(@select.ObjectItem.Count)?.UserId ?? 0;
		}
		private long FollowBasedOnTopic()
		{
			var by = new By
			{
				ActionType = (int)ActionType.FollowUser,
				User = user.Profile.InstagramAccountId
			};
			var fetchMedias = _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaByTopic, user.Profile.ProfileTopic.Category._id)
				.GetAwaiter().GetResult().Where(exclude=>!exclude.SeenBy.Any(e=>e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Medias.Count > 0);
			if (fetchMedias == null) return 0;
			var meta_S = fetchMedias as __Meta__<Media>[] ?? fetchMedias.ToArray();
			var select = meta_S.ElementAtOrDefault(SecureRandom.Next(meta_S.Count()));
			if (@select == null) return 0;
			@select.SeenBy.Add(@by);
			_heartbeatLogic.UpdateMetaData(MetaDataType.FetchMediaByTopic, user.Profile.ProfileTopic.Category._id, @select).GetAwaiter().GetResult();
			return @select.ObjectItem?.Medias.FirstOrDefault()?.User.UserId ?? 0;
		}
		private long FollowBasedOnLocation()
		{
			var by = new By
			{
				ActionType = (int)ActionType.FollowUser,
				User = user.Profile.InstagramAccountId
			};
			var fetchMedias = _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaByUserLocationTargetList, user.Profile.ProfileTopic.Category._id,user.Profile.InstagramAccountId)
				.GetAwaiter().GetResult().Where(exclude=>!exclude.SeenBy.Any(e=>e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Medias.Count > 0);
			var meta_S = fetchMedias as __Meta__<Media>[] ?? fetchMedias.ToArray();
			var select = meta_S.ElementAtOrDefault(SecureRandom.Next(meta_S.Count()));
			if (@select == null) return 0;
			@select.SeenBy.Add(@by);
			_heartbeatLogic.UpdateMetaData(MetaDataType.FetchMediaByUserLocationTargetList, user.Profile.ProfileTopic.Category._id, @select,user.Profile.InstagramAccountId).GetAwaiter().GetResult();
			return @select.ObjectItem.Medias.FirstOrDefault()?.User.UserId ?? 0;
		}
		private long FollowBasedOnCommenters()
		{
			var by = new By
			{
				ActionType = (int)ActionType.FollowUser,
				User = user.Profile.InstagramAccountId
			};
			var fetchMedias = _heartbeatLogic.GetMetaData<List<UserResponse<CommentResponse>>>(MetaDataType.FetchUsersViaPostCommented, user.Profile.ProfileTopic.Category._id)
				.GetAwaiter().GetResult().Where(exclude=>!exclude.SeenBy.Any(e=>e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Count > 0);
			var meta_S = fetchMedias as __Meta__<List<UserResponse<CommentResponse>>>[] ?? fetchMedias.ToArray();
			var select = meta_S.ElementAtOrDefault(SecureRandom.Next(meta_S.Count()));
			if (@select == null) return 0;
			@select.SeenBy.Add(@by);
			_heartbeatLogic.UpdateMetaData(MetaDataType.FetchUsersViaPostCommented, user.Profile.ProfileTopic.Category._id, @select).GetAwaiter().GetResult();
			return @select.ObjectItem.ElementAtOrDefault(@select.ObjectItem.Count)?.UserId ?? 0;
		}
		private long FollowBasedOnSuggestions()
		{
			var by = new By
			{
				ActionType = (int)ActionType.FollowUser,
				User = user.Profile.InstagramAccountId
			};
			var fetchMedias = _heartbeatLogic.GetMetaData<List<UserResponse<UserSuggestionDetails>>>(MetaDataType.FetchUsersFollowSuggestions, user.Profile.ProfileTopic.Category._id, user.Profile.InstagramAccountId)
				.GetAwaiter().GetResult().Where(exclude=>!exclude.SeenBy.Any(e=>e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Count > 0);

			var meta_S = fetchMedias as __Meta__<List<UserResponse<UserSuggestionDetails>>>[] ?? fetchMedias.ToArray();
			var select = meta_S.ElementAtOrDefault(SecureRandom.Next(meta_S.Count()));
			if (@select == null) return 0;
			@select.SeenBy.Add(@by);

			_heartbeatLogic.UpdateMetaData(MetaDataType.FetchUsersFollowSuggestions, user.Profile.ProfileTopic.Category._id, @select).GetAwaiter().GetResult();
			return @select.ObjectItem.ElementAtOrDefault(@select.ObjectItem.Count)?.UserId ?? 0;
		}
		public ResultCarrier<IEnumerable<TimelineEventModel>> Push(IActionOptions actionOptions)
		{
			Console.WriteLine($"Follow Action Started: {user.OAccountId}, {user.OInstagramAccountUsername}, {user.OInstagramAccountUser}");
			var Results = new ResultCarrier<IEnumerable<TimelineEventModel>>();
			var followActionOptions = actionOptions as FollowActionOptions; 
			if(user == null)
			{
				Results.IsSuccesful = false;
				Results.Info = new ErrorResponse
				{
					Message = $"user is null, user: {user.OAccountId}, instaId: {user.OInstagramAccountUsername}",
					StatusCode = System.Net.HttpStatusCode.NotFound
				};
				return Results;
			}

			var followActionTypeSelected = FollowActionType.FollowBasedOnTopic;
			try
			{
				long nominatedFollower = 0;
				//todo add Location?
				var followActionsChances = new List<Chance<FollowActionType>>();
				if (followActionOptions != null && followActionOptions.FollowActionType == FollowActionType.Any)
				{
					if (user.Profile.LocationTargetList != null && user.Profile?.LocationTargetList?.Count > 0)
					{
						followActionsChances.AddRange(new List<Chance<FollowActionType>>
						{
							new Chance<FollowActionType>
							{
								Object = FollowActionType.FollowBasedOnCommenters, 
								Probability = 0.20
							},
							new Chance<FollowActionType>
							{
								Object = FollowActionType.FollowBasedOnLocation, 
								Probability = user.Profile.AdditionalConfigurations.FocusLocalMore ? 0.40 : 0.15
							},
							new Chance<FollowActionType>
							{
								Object = FollowActionType.FollowBasedOnLikers, 
								Probability = user.Profile.AdditionalConfigurations.FocusLocalMore ? 0.15 : 0.40
							},
							new Chance<FollowActionType>
							{
								Object = FollowActionType.FollowBasedOnTopic, 
								Probability = 0.5
							},
							new Chance<FollowActionType>
							{
								Object = FollowActionType.FollowBasedOnSuggestions,
								Probability = 0.20
							}
						});
					}
					else
					{
						followActionsChances.AddRange(new List<Chance<FollowActionType>>
						{
							new Chance<FollowActionType>
							{
								Object = FollowActionType.FollowBasedOnCommenters, 
								Probability = 0.25
							},
							new Chance<FollowActionType>
							{
								Object = FollowActionType.FollowBasedOnLikers, 
								Probability = 0.40
							},
							new Chance<FollowActionType>
							{
								Object = FollowActionType.FollowBasedOnTopic, 
								Probability = 0.15
							},
							new Chance<FollowActionType>
							{
								Object = FollowActionType.FollowBasedOnSuggestions,
								Probability = 0.20
							}
						});
					}

					followActionTypeSelected = SecureRandom.ProbabilityRoll(followActionsChances);
				}
				else
				{
					if (followActionOptions != null) followActionTypeSelected = followActionOptions.FollowActionType;
				}
				switch (followActionTypeSelected)
				{
					case FollowActionType.FollowBasedOnCommenters:
						nominatedFollower = FollowBasedOnCommenters();
						break;
					case FollowActionType.FollowBasedOnLikers:
						nominatedFollower = FollowBasedOnLikers();
						break;
					case FollowActionType.FollowBasedOnTopic:
						nominatedFollower = FollowBasedOnTopic();
						break;
					case FollowActionType.FollowBasedOnLocation:
						nominatedFollower = FollowBasedOnLocation();
						break;
					case FollowActionType.FollowBasedOnSuggestions:
						nominatedFollower = FollowBasedOnSuggestions();
						break;
				}
				if (nominatedFollower == 0){
					Results.IsSuccesful = false;
					Results.Info = new ErrorResponse
					{
						Message = $"could not find a nominated person to follow, user: {user.OAccountId}, instaId: {user.OInstagramAccountUsername}",
						StatusCode = System.Net.HttpStatusCode.NotFound
					};
					return Results;
				}

				var restModel = new RestModel
				{
					BaseUrl = string.Format(_urlReader.FollowUser, nominatedFollower),
					RequestType = RequestType.POST,
					JsonBody = null,
					User = user
				};
				Results.IsSuccesful = true;
				Results.Results = new List<TimelineEventModel>
				{
					new TimelineEventModel
					{
						ActionName = $"FollowUser_{followStrategySettings.FollowStrategy.ToString()}_{followActionTypeSelected.ToString()}",
						Data = restModel,
						ExecutionTime =followActionOptions.ExecutionTime
					}
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

		public IActionCommit IncludeStorage(IStorage storage)
		{
			throw new NotImplementedException();
		}
	}
}
