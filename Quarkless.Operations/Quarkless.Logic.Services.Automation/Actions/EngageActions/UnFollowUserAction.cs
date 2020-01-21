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
using Quarkless.Models.Services.Automation.Enums.Actions.StrategyType;
using Quarkless.Models.Services.Automation.Interfaces;
using Quarkless.Models.Services.Automation.Models.ActionOptions;
using Quarkless.Models.Services.Automation.Models.StrategySettings;
using Quarkless.Models.Storage.Interfaces;
using Quarkless.Models.Timeline;

namespace Quarkless.Logic.Services.Automation.Actions.EngageActions
{
	class UnFollowUserAction : IActionCommit
	{
		private readonly IContentInfoBuilder _content;
		private readonly IHeartbeatLogic _heartbeatLogic;
		private readonly IUrlReader _urlReader;
		private UnFollowStrategySettings unFollowStrategySettings;
		private UserStoreDetails user;
		public UnFollowUserAction(IContentInfoBuilder content,IHeartbeatLogic heartbeatLogic, IUrlReader urlReader)
		{
			this._content = content;
			this._heartbeatLogic = heartbeatLogic;
			this._urlReader = urlReader;
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
				results.IsSuccessful = false;
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
			var fetchedUsers = _heartbeatLogic.GetMetaData<List<UserResponse<string>>>(new MetaDataFetchRequest
				{
					MetaDataType = MetaDataType.FetchUsersFollowingList,
					ProfileCategoryTopicId = user.Profile.ProfileTopic.Category._id,
					InstagramId = user.ShortInstagram.Id
				})
				.GetAwaiter().GetResult().Where(exclude => !exclude.SeenBy.Any(e => e.User == by.User && e.ActionType == by.ActionType));
			var meta_S = fetchedUsers as Meta<List<UserResponse<string>>>[] ?? fetchedUsers.ToArray();
			switch (unFollowStrategySettings.UnFollowStrategy)
			{
				case UnFollowStrategyType.Default:
				{
					var nominatedUser = meta_S.ElementAt(SecureRandom.Next(meta_S.Count()));
					//todo check if this user has been engaging with the user (commenting, liking, seeing story, etc)
					if (nominatedUser!=null)
					{
						nominatedUser.SeenBy.Add(@by);
						_heartbeatLogic.UpdateMetaData(new MetaDataCommitRequest<List<UserResponse<string>>>
						{
							MetaDataType = MetaDataType.FetchUsersFollowingList,
							ProfileCategoryTopicId = user.Profile.ProfileTopic.Category._id,
							InstagramId = user.ShortInstagram.Id,
							Data = nominatedUser
						}).GetAwaiter().GetResult();
						var restModel = new RestModel
						{
							BaseUrl = string.Format(_urlReader.UnfollowUser, nominatedUser.ObjectItem.FirstOrDefault()?.UserId),
							RequestType = RequestType.Post,
							JsonBody = null,
							User = user
						};
						results.IsSuccessful = true;
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
					var events = new List<TimelineEventModel>();
					for (var i = 0; i < unFollowStrategySettings.NumberOfUnfollows; i++)
					{
						var nominate = meta_S.ElementAtOrDefault(i);
						if (nominate == null) continue;
						nominate.SeenBy.Add(@by);
						_heartbeatLogic.UpdateMetaData(new MetaDataCommitRequest<List<UserResponse<string>>>
						{
							MetaDataType = MetaDataType.FetchUsersFollowingList,
							ProfileCategoryTopicId = user.Profile.ProfileTopic.Category._id,
							InstagramId = user.ShortInstagram.Id,
							Data = nominate
						}).GetAwaiter().GetResult();
						var restModel = new RestModel
						{
							BaseUrl = string.Format(_urlReader.UnfollowUser, nominate.ObjectItem.FirstOrDefault()?.UserId),
							RequestType = RequestType.Post,
							JsonBody = null,
							User = user
						};
						events.Add(new TimelineEventModel
						{
							ActionName = $"UnfollowUser_{unFollowStrategySettings.UnFollowStrategy.ToString()}",
							Data = restModel,
							ExecutionTime = followActionOptions.ExecutionTime.AddSeconds(i*unFollowStrategySettings.OffsetPerAction.TotalSeconds)
						});
					}
					results.IsSuccessful = true;
					results.Results = events;
					return results;
				}
			}
			results.IsSuccessful = false;
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
