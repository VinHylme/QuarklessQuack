﻿using Quarkless.Services.Factories;
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
using QuarklessLogic.Logic.StorageLogic;

namespace Quarkless.Services.ActionBuilders.EngageActions
{
	class UnFollowUserAction : IActionCommit
	{
		private readonly IContentManager _content;
		private readonly IHeartbeatLogic _heartbeatLogic;
		private UnFollowStrategySettings unFollowStrategySettings;
		private UserStoreDetails user;
		public UnFollowUserAction(IContentManager content,IHeartbeatLogic heartbeatLogic)
		{
			this._content = content;
			this._heartbeatLogic = heartbeatLogic;
		}

		public IActionCommit IncludeStrategy(IStrategySettings strategy)
		{
			this.unFollowStrategySettings = strategy as UnFollowStrategySettings;
			return this;
		}
		public ResultCarrier<IEnumerable<TimelineEventModel>> Push(IActionOptions actionOptions)
		{
			Console.WriteLine($"Unfollow Action Started: {user.OAccountId}, {user.OInstagramAccountUsername}, {user.OInstagramAccountUser}");

			var results = new ResultCarrier<IEnumerable<TimelineEventModel>>();
			var followActionOptions = actionOptions as UnfollowActionOptions;
			if(user==null)
			{
				results.IsSuccesful = false;
				results.Info = new ErrorResponse
				{
					Message = $"user is null, user: {user.OAccountId}, instaId: {user.OInstagramAccountUsername}",
					StatusCode = System.Net.HttpStatusCode.NotFound
				};
				return results;
			}
			var by = new By
			{
				ActionType = (int)ActionType.UnFollowUser,
				User = user.Profile.InstagramAccountId
			};
			var fetchedUsers = _heartbeatLogic.GetMetaData<List<UserResponse<string>>>(MetaDataType.FetchUsersFollowingList, user.Profile.Topics.TopicFriendlyName, user.Profile.InstagramAccountId)
				.GetAwaiter().GetResult().Where(exclude => !exclude.SeenBy.Any(e => e.User == by.User && e.ActionType == by.ActionType));
			var meta_S = fetchedUsers as __Meta__<List<UserResponse<string>>>[] ?? fetchedUsers.ToArray();
			switch (unFollowStrategySettings.UnFollowStrategy)
			{
				case UnFollowStrategyType.Default:
				{
					var nominatedUser = meta_S.ElementAt(SecureRandom.Next(meta_S.Count()));
					//todo check if this user has been engaging with the user (commenting, liking, seeing story, etc)
					if (nominatedUser!=null)
					{
						nominatedUser.SeenBy.Add(@by);
						_heartbeatLogic.UpdateMetaData(MetaDataType.FetchUsersFollowingList, user.Profile.Topics.TopicFriendlyName, nominatedUser).GetAwaiter().GetResult();
						var restModel = new RestModel
						{
							BaseUrl = string.Format(UrlConstants.UnfollowUser, nominatedUser.ObjectItem.FirstOrDefault()?.UserId),
							RequestType = RequestType.POST,
							JsonBody = null,
							User = user
						};
						results.IsSuccesful = true;
						results.Results = new List<TimelineEventModel>
						{
							new TimelineEventModel
							{
								ActionName = $"UnfollowUser_{unFollowStrategySettings.UnFollowStrategy.ToString()}",
								Data = restModel,
								ExecutionTime =followActionOptions.ExecutionTime
							}
						};
						return results;
					}

					break;
				}
				case UnFollowStrategyType.LeastEngagingN:
				{
					var events_ = new List<TimelineEventModel>();
					for (var i = 0; i < unFollowStrategySettings.NumberOfUnfollows; i++)
					{
						var nominate = meta_S.ElementAtOrDefault(i);
						if (nominate == null) continue;
						nominate.SeenBy.Add(@by);
						_heartbeatLogic.UpdateMetaData(MetaDataType.FetchUsersFollowingList, user.Profile.Topics.TopicFriendlyName, nominate).GetAwaiter().GetResult();
						var restModel = new RestModel
						{
							BaseUrl = string.Format(UrlConstants.UnfollowUser, nominate.ObjectItem.FirstOrDefault()?.UserId),
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
					results.IsSuccesful = true;
					results.Results = events_;
					return results;
				}
			}
			results.IsSuccesful = false;
			results.Info = new ErrorResponse
			{
				Message = $"strategy not implemented yet, user: {user.OAccountId}, instaId: {user.OInstagramAccountUsername}",
				StatusCode = System.Net.HttpStatusCode.Forbidden
			};
			return results;
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
