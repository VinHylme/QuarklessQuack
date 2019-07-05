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
using System.Text;

namespace Quarkless.Services.ActionBuilders.EngageActions
{
	class UnFollowUserAction : IActionCommit
	{
		private readonly IContentManager _content;
		private readonly IHeartbeatLogic _heartbeatLogic;
		private UnFollowStrategySettings unFollowStrategySettings;
		private readonly ProfileModel _profile;
		private UserStoreDetails user;
		public UnFollowUserAction(IContentManager content,IHeartbeatLogic heartbeatLogic, ProfileModel profile)
		{
			this._content = content;
			this._profile = profile;
			this._heartbeatLogic = heartbeatLogic;
		}

		public IActionCommit IncludeStrategy(IStrategySettings strategy)
		{
			this.unFollowStrategySettings = strategy as UnFollowStrategySettings;
			return this;
		}
		public IEnumerable<TimelineEventModel> Push(IActionOptions actionOptions)
		{
			FollowActionOptions followActionOptions = actionOptions as FollowActionOptions;
			if(user==null) return null;
			if(unFollowStrategySettings.UnFollowStrategy == UnFollowStrategyType.Default)
			{
				var fetchedUsers = _heartbeatLogic.GetMetaData<List<UserResponse<string>>>(MetaDataType.FetchUsersFollowingList, _profile.Topic, _profile.InstagramAccountId).GetAwaiter().GetResult();

				if (fetchedUsers != null)
				{
					var nominatedUser = fetchedUsers.ElementAt(SecureRandom.Next(fetchedUsers.Count()));
					//todo check if this user has been engaging with the user (commenting, liking, seeing story, etc)

					if (nominatedUser.HasValue)
					{
						By by = new By
						{
							ActionType = (int)ActionType.UnFollowUser,
							User = _profile.InstagramAccountId
						};
						if (!nominatedUser.Value.SeenBy.Contains(by))
						{
							nominatedUser.Value.SeenBy.Add(by);
							_heartbeatLogic.UpdateMetaData(MetaDataType.FetchUsersFollowingList, _profile.Topic, nominatedUser.Value).GetAwaiter().GetResult();
							RestModel restModel = new RestModel
							{
								BaseUrl = string.Format(UrlConstants.UnfollowUser, nominatedUser.Value.ObjectItem.FirstOrDefault().UserId),
								RequestType = RequestType.POST,
								JsonBody = null,
								User = user
							};
							return new List<TimelineEventModel>
							{
								new TimelineEventModel
								{
									ActionName = $"FollowUser_{unFollowStrategySettings.UnFollowStrategy.ToString()}",
									Data = restModel,
									ExecutionTime =followActionOptions.ExecutionTime
								}
							};
						}
					}			
				}
			}
			else if(unFollowStrategySettings.UnFollowStrategy == UnFollowStrategyType.LeastEngagingN)
			{

			}
			return null;
		}

		public IActionCommit IncludeUser(UserStoreDetails userStoreDetails)
		{
			user = userStoreDetails;
			return this;
		}
	}
}
