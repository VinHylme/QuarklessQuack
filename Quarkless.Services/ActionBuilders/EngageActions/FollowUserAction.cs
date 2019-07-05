using Newtonsoft.Json;
using Quarkless.Services.Factories;
using Quarkless.Services.Interfaces;
using Quarkless.Services.Interfaces.Actions;
using Quarkless.Services.StrategyBuilders;
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
		FollowBasedOnTopic = 3
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
			var fetchMedias = _heartbeatLogic.GetMetaData<List<UserResponse<string>>>(MetaDataType.FetchUsersViaPostLiked, _profile.Topic).GetAwaiter().GetResult();
			if (fetchMedias != null)
			{
				var select = fetchMedias.ElementAtOrDefault(SecureRandom.Next(fetchMedias.Count()));
				if (select.HasValue)
				{
					By by = new By
					{
						ActionType = (int)ActionType.FollowUser,
						User = _profile.InstagramAccountId
					};
					if (!select.Value.SeenBy.Contains(by))
					{
						select.Value.SeenBy.Add(by);
						_heartbeatLogic.UpdateMetaData(MetaDataType.FetchUsersViaPostLiked, _profile.Topic, select.Value).GetAwaiter().GetResult();
						return select.Value.ObjectItem.ElementAtOrDefault(select.Value.ObjectItem.Count).UserId;
					}
				}
			}
			return 0;
		}
		private long FollowBasedOnTopic()
		{
			var fetchMedias = _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaByTopic, _profile.Topic).GetAwaiter().GetResult();
			if (fetchMedias != null)
			{
				var select = fetchMedias.ElementAtOrDefault(SecureRandom.Next(fetchMedias.Count()));
				if (select.HasValue)
				{
					By by = new By
					{
						ActionType = (int)ActionType.FollowUser,
						User = _profile.InstagramAccountId
					};
					if (!select.Value.SeenBy.Contains(by))
					{
						select.Value.SeenBy.Add(by);
						_heartbeatLogic.UpdateMetaData(MetaDataType.FetchMediaByTopic, _profile.Topic, select.Value).GetAwaiter().GetResult();
						return select.Value.ObjectItem.Medias.FirstOrDefault().User.UserId;
					}
				}
			}
			return 0;
		}
		private long FollowBasedOnCommenters()
		{
			var fetchMedias = _heartbeatLogic.GetMetaData<List<UserResponse<CommentResponse>>>(MetaDataType.FetchUsersViaPostCommented, _profile.Topic).GetAwaiter().GetResult();
			if (fetchMedias != null)
			{
				var select = fetchMedias.ElementAtOrDefault(SecureRandom.Next(fetchMedias.Count()));
				if (select.HasValue)
				{
					By by = new By
					{
						ActionType = (int)ActionType.FollowUser,
						User = _profile.InstagramAccountId
					};
					if (!select.Value.SeenBy.Contains(by))
					{
						select.Value.SeenBy.Add(by);
						_heartbeatLogic.UpdateMetaData(MetaDataType.FetchUsersViaPostCommented, _profile.Topic, select.Value).GetAwaiter().GetResult();
						return select.Value.ObjectItem.ElementAtOrDefault(select.Value.ObjectItem.Count).UserId;
					}
				}
			}
			return 0;
		}
		public IEnumerable<TimelineEventModel> Push(IActionOptions actionOptions)
		{
			FollowActionOptions followActionOptions = actionOptions as FollowActionOptions; 
			if(user == null) return null;
			try
			{
				Console.WriteLine("Follow Action Started");
				long nominatedFollower = 0;
				//todo add Location?
				FollowActionType followActionTypeSelected = FollowActionType.FollowBasedOnTopic;
				if (followActionOptions.FollowActionType == FollowActionType.Any)
				{
					List<Chance<FollowActionType>> followActionsChances = new List<Chance<FollowActionType>>
					{
						new Chance<FollowActionType>{Object = FollowActionType.FollowBasedOnCommenters, Probability = 0.30},
						new Chance<FollowActionType>{Object = FollowActionType.FollowBasedOnLikers, Probability = 0.55},
						new Chance<FollowActionType>{Object = FollowActionType.FollowBasedOnTopic, Probability = 0.15}
					};
					followActionTypeSelected = SecureRandom.ProbabilityRoll(followActionsChances);
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
				}
				if (nominatedFollower == 0) return null;

				RestModel restModel = new RestModel
				{
					BaseUrl = string.Format(UrlConstants.FollowUser, nominatedFollower),
					RequestType = RequestType.POST,
					JsonBody = null,
					User = user
				};
				return new List<TimelineEventModel>
				{
					new TimelineEventModel
					{
						ActionName = $"FollowUser_{followStrategySettings.FollowStrategy.ToString()}_{followActionTypeSelected.ToString()}",
						Data = restModel,
						ExecutionTime =followActionOptions.ExecutionTime
					}
				};
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
				return null;
			}
		}

		public IActionCommit IncludeUser(UserStoreDetails userStoreDetails)
		{
			user = userStoreDetails;
			return this;
		}
	}
}
