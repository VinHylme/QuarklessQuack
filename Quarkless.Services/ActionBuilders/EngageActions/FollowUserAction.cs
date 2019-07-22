using Newtonsoft.Json;
using Quarkless.Services.Factories;
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
using System.Threading;

namespace Quarkless.Services.ActionBuilders.EngageActions
{
	public enum FollowActionType
	{
		Any = 0,
		FollowBasedOnLikers = 1,
		FollowBasedOnCommenters = 2,
		FollowBasedOnTopic = 3,
		FollowBasedOnLocation = 4 
	}
	public class FollowUserAction : IActionCommit
	{
		private readonly IContentManager _builder;
		private readonly IHeartbeatLogic _heartbeatLogic;
		private readonly ProfileModel _profile;
		private UserStoreDetails user;
		private FollowStrategySettings followStrategySettings;
		public FollowUserAction(IContentManager builder, IHeartbeatLogic heartbeatLogic, ProfileModel profile)
		{
			_builder = builder;
			_profile = profile;
			_heartbeatLogic = heartbeatLogic;
		}
		public IActionCommit IncludeStrategy(IStrategySettings strategy)
		{
			this.followStrategySettings = strategy as FollowStrategySettings;
			return this;
		}
		private long FollowBasedOnLikers()
		{
			By by = new By
			{
				ActionType = (int)ActionType.FollowUser,
				User = _profile.InstagramAccountId
			};
			var fetchMedias = _heartbeatLogic.GetMetaData<List<UserResponse<string>>>(MetaDataType.FetchUsersViaPostLiked, _profile.Topics.TopicFriendlyName)
				.GetAwaiter().GetResult().Where(exclude=>!exclude.SeenBy.Any(e=>e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Count > 0);
			if (fetchMedias != null)
			{
				var select = fetchMedias.ElementAtOrDefault(SecureRandom.Next(fetchMedias.Count()));
				if (select!=null)
				{
					select.SeenBy.Add(by);
					_heartbeatLogic.UpdateMetaData(MetaDataType.FetchUsersViaPostLiked, _profile.Topics.TopicFriendlyName, select).GetAwaiter().GetResult();
					return select.ObjectItem.ElementAtOrDefault(select.ObjectItem.Count-1).UserId;
				}
			}
			return 0;
		}
		private long FollowBasedOnTopic()
		{
			By by = new By
			{
				ActionType = (int)ActionType.FollowUser,
				User = _profile.InstagramAccountId
			};
			var fetchMedias = _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaByTopic, _profile.Topics.TopicFriendlyName)
				.GetAwaiter().GetResult().Where(exclude=>!exclude.SeenBy.Any(e=>e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Medias.Count > 0);
			if (fetchMedias != null)
			{
				var select = fetchMedias.ElementAtOrDefault(SecureRandom.Next(fetchMedias.Count()));
				if (select!=null)
				{
					select.SeenBy.Add(by);
					_heartbeatLogic.UpdateMetaData(MetaDataType.FetchMediaByTopic, _profile.Topics.TopicFriendlyName, select).GetAwaiter().GetResult();
					return select.ObjectItem.Medias.FirstOrDefault().User.UserId;		
				}
			}
			return 0;
		}
		private long FollowBasedOnLocation()
		{
			By by = new By
			{
				ActionType = (int)ActionType.FollowUser,
				User = _profile.InstagramAccountId
			};
			var fetchMedias = _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaByUserLocationTargetList, _profile.Topics.TopicFriendlyName,_profile.InstagramAccountId)
				.GetAwaiter().GetResult().Where(exclude=>!exclude.SeenBy.Any(e=>e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Medias.Count > 0);
			if (fetchMedias != null)
			{
				var select = fetchMedias.ElementAtOrDefault(SecureRandom.Next(fetchMedias.Count()));
				if (select!=null)
				{
					select.SeenBy.Add(by);
					_heartbeatLogic.UpdateMetaData(MetaDataType.FetchMediaByUserLocationTargetList, _profile.Topics.TopicFriendlyName, select,_profile.InstagramAccountId).GetAwaiter().GetResult();
					return select.ObjectItem.Medias.FirstOrDefault().User.UserId;	
				}
			}
			return 0;
		}
		private long FollowBasedOnCommenters()
		{
			By by = new By
			{
				ActionType = (int)ActionType.FollowUser,
				User = _profile.InstagramAccountId
			};
			var fetchMedias = _heartbeatLogic.GetMetaData<List<UserResponse<CommentResponse>>>(MetaDataType.FetchUsersViaPostCommented, _profile.Topics.TopicFriendlyName)
				.GetAwaiter().GetResult().Where(exclude=>!exclude.SeenBy.Any(e=>e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Count > 0);
			if (fetchMedias != null)
			{
				var select = fetchMedias.ElementAtOrDefault(SecureRandom.Next(fetchMedias.Count()));
				if (select!=null)
				{
					select.SeenBy.Add(by);
					_heartbeatLogic.UpdateMetaData(MetaDataType.FetchUsersViaPostCommented, _profile.Topics.TopicFriendlyName, select).GetAwaiter().GetResult();
					return select.ObjectItem.ElementAtOrDefault(select.ObjectItem.Count-1).UserId;
				}
			}
			return 0;
		}
		public ResultCarrier<IEnumerable<TimelineEventModel>> Push(IActionOptions actionOptions)
		{
			Console.WriteLine("Follow Action Started");
			ResultCarrier<IEnumerable<TimelineEventModel>> Results = new ResultCarrier<IEnumerable<TimelineEventModel>>();
			FollowActionOptions followActionOptions = actionOptions as FollowActionOptions; 
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
			try
			{
				long nominatedFollower = 0;
				//todo add Location?
				FollowActionType followActionTypeSelected = FollowActionType.FollowBasedOnTopic;
				if (followActionOptions.FollowActionType == FollowActionType.Any)
				{
					List<Chance<FollowActionType>> followActionsChances = new List<Chance<FollowActionType>>
					{
						new Chance<FollowActionType>{Object = FollowActionType.FollowBasedOnCommenters, Probability = 0.25},
						new Chance<FollowActionType>{Object = FollowActionType.FollowBasedOnLikers, Probability = 0.40},
						new Chance<FollowActionType>{Object = FollowActionType.FollowBasedOnTopic, Probability = 0.15}
					};
					if (_profile.LocationTargetList != null)
						if (_profile.LocationTargetList.Count > 0)
							followActionsChances.Add(new Chance<FollowActionType> { Object = FollowActionType.FollowBasedOnLocation, Probability = 0.20 });
				}
				else
				{
					followActionTypeSelected = followActionOptions.FollowActionType;
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

				RestModel restModel = new RestModel
				{
					BaseUrl = string.Format(UrlConstants.FollowUser, nominatedFollower),
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
	}
}
