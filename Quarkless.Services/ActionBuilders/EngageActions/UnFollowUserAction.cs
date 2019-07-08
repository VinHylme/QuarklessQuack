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
			FollowActionOptions followActionOptions = actionOptions as FollowActionOptions;
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
			if(unFollowStrategySettings.UnFollowStrategy == UnFollowStrategyType.Default)
			{
				var fetchedUsers = _heartbeatLogic.GetMetaData<List<UserResponse<string>>>(MetaDataType.FetchUsersFollowingList, _profile.Topics.TopicFriendlyName, _profile.InstagramAccountId).GetAwaiter().GetResult();

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
							_heartbeatLogic.UpdateMetaData(MetaDataType.FetchUsersFollowingList, _profile.Topics.TopicFriendlyName, nominatedUser.Value).GetAwaiter().GetResult();
							RestModel restModel = new RestModel
							{
								BaseUrl = string.Format(UrlConstants.UnfollowUser, nominatedUser.Value.ObjectItem.FirstOrDefault().UserId),
								RequestType = RequestType.POST,
								JsonBody = null,
								User = user
							};
							Results.IsSuccesful = true;
							Results.Results = new List<TimelineEventModel>
							{
								new TimelineEventModel
								{
									ActionName = $"FollowUser_{unFollowStrategySettings.UnFollowStrategy.ToString()}",
									Data = restModel,
									ExecutionTime =followActionOptions.ExecutionTime
								}
							};
							return Results;
						}
					}			
				}
			}
			else if(unFollowStrategySettings.UnFollowStrategy == UnFollowStrategyType.LeastEngagingN)
			{

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
