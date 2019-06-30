using Quarkless.Services.Factories;
using Quarkless.Services.Interfaces;
using Quarkless.Services.Interfaces.Actions;
using Quarkless.Services.StrategyBuilders;
using QuarklessContexts.Enums;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.ServicesModels.SearchModels;
using QuarklessContexts.Models.Timeline;
using QuarklessLogic.Handlers.RequestBuilder.Consts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quarkless.Services.ActionBuilders.EngageActions
{
	class UnFollowUserAction : IActionCommit
	{
		private readonly IContentManager _content;
		private UnFollowStrategySettings unFollowStrategySettings;
		private readonly ProfileModel _profile;
		private UserStoreDetails user;
		public UnFollowUserAction(IContentManager content, ProfileModel profile)
		{
			this._content = content;
			this._profile = profile;
		}

		public IActionCommit IncludeStrategy(IStrategySettings strategy)
		{
			this.unFollowStrategySettings = strategy as UnFollowStrategySettings;
			return this;
		}
		private bool HasUserEngaged(long userId)
		{
			var findNominatedUser = _content.SearchInstagramFullUserDetail(userId);


			return false;
		}
		public IEnumerable<TimelineEventModel> Push(IActionOptions actionOptions)
		{
			FollowActionOptions followActionOptions = actionOptions as FollowActionOptions;
			if(user==null) return null;
			if(unFollowStrategySettings.UnFollowStrategy == UnFollowStrategyType.Default)
			{
				var fetchedItems =  MediaFetcherManager.Begin.Commit(FetchType.Users,_content,_profile).FetchUsers(2, OtherActions.UserFetchType.UserFollowingList).FetchedItems;
				if (fetchedItems != null)
				{
					var users = (IEnumerable<UserResponse<string>>) fetchedItems;
					var nominated = users.ElementAt(SecureRandom.Next(users.Count() - 1)).UserId;
					if (!HasUserEngaged(nominated))
					{
						RestModel restModel = new RestModel
						{
							BaseUrl = string.Format(UrlConstants.UnfollowUser, nominated),
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
