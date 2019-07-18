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
		public ResultCarrier<IEnumerable<TimelineEventModel>> Push(IActionOptions actionOptions)
		{
			ResultCarrier<IEnumerable<TimelineEventModel>> Results = new ResultCarrier<IEnumerable<TimelineEventModel>>();
			UnfollowActionOptions followActionOptions = actionOptions as UnfollowActionOptions;
			if(user==null)
			{
				Results.IsSuccesful = false;
				Results.Info = new ErrorResponse
				{
					Message = $"user is null, user: {user.OAccountId}, instaId: {user.OInstagramAccountUsername}",
					StatusCode = System.Net.HttpStatusCode.NotFound
				};
				return Results;
			}
			By by = new By
			{
				ActionType = (int)ActionType.UnFollowUser,
				User = _profile.InstagramAccountId
			};
			var fetchedUsers = _heartbeatLogic.GetMetaData<List<UserResponse<string>>>(MetaDataType.FetchUsersFollowingList, _profile.Topics.TopicFriendlyName, _profile.InstagramAccountId)
				.GetAwaiter().GetResult().Where(exclude => !exclude.SeenBy.Any(e => e.User == by.User && e.ActionType == by.ActionType));
			if (fetchedUsers != null) { 
				if (unFollowStrategySettings.UnFollowStrategy == UnFollowStrategyType.Default)
				{
					var nominatedUser = fetchedUsers.ElementAt(SecureRandom.Next(fetchedUsers.Count()));
					//todo check if this user has been engaging with the user (commenting, liking, seeing story, etc)
					if (nominatedUser!=null)
					{
						nominatedUser.SeenBy.Add(by);
						_heartbeatLogic.UpdateMetaData(MetaDataType.FetchUsersFollowingList, _profile.Topics.TopicFriendlyName, nominatedUser).GetAwaiter().GetResult();
						RestModel restModel = new RestModel
						{
							BaseUrl = string.Format(UrlConstants.UnfollowUser, nominatedUser.ObjectItem.FirstOrDefault().UserId),
							RequestType = RequestType.POST,
							JsonBody = null,
							User = user
						};
						Results.IsSuccesful = true;
						Results.Results = new List<TimelineEventModel>
						{
							new TimelineEventModel
							{
								ActionName = $"UnfollowUser_{unFollowStrategySettings.UnFollowStrategy.ToString()}",
								Data = restModel,
								ExecutionTime =followActionOptions.ExecutionTime
							}
						};
						return Results;
					}		
				}
				else if(unFollowStrategySettings.UnFollowStrategy == UnFollowStrategyType.LeastEngagingN)
				{
					List<TimelineEventModel> events_ = new List<TimelineEventModel>();
					for (int i = 0; i < unFollowStrategySettings.NumberOfUnfollows; i++)
					{
						var nominate = fetchedUsers.ElementAtOrDefault(i);
						if (nominate != null)
						{
							nominate.SeenBy.Add(by);
							_heartbeatLogic.UpdateMetaData(MetaDataType.FetchUsersFollowingList, _profile.Topics.TopicFriendlyName, nominate).GetAwaiter().GetResult();
							RestModel restModel = new RestModel
							{
								BaseUrl = string.Format(UrlConstants.UnfollowUser, nominate.ObjectItem.FirstOrDefault().UserId),
								RequestType = RequestType.POST,
								JsonBody = null,
								User = user
							};
							events_.Add(new TimelineEventModel
							{
								ActionName = $"UnfollowUser_{unFollowStrategySettings.UnFollowStrategy.ToString()}",
								Data = restModel,
								ExecutionTime = followActionOptions.ExecutionTime.AddSeconds(i*unFollowStrategySettings.OffsetPerAction.TotalSeconds)
							});
						}
						continue;
					}
					Results.IsSuccesful = true;
					Results.Results = events_;
					return Results;
				}
			}
			Results.IsSuccesful = false;
			Results.Info = new ErrorResponse
			{
				Message = $"strategy not implemented yet, user: {user.OAccountId}, instaId: {user.OInstagramAccountUsername}",
				StatusCode = System.Net.HttpStatusCode.Forbidden
			};
			return Results;
		}

		public IActionCommit IncludeUser(UserStoreDetails userStoreDetails)
		{
			user = userStoreDetails;
			return this;
		}
	}
}
