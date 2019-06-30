using Newtonsoft.Json;
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
		private readonly ProfileModel _profile;
		private UserStoreDetails user;
		private FollowStrategySettings followStrategySettings;
		public FollowUserAction(IContentManager builder, ProfileModel profile)
		{
			_builder = builder;
			_profile = profile;
		}
		public IActionCommit IncludeStrategy(IStrategySettings strategy)
		{
			this.followStrategySettings = strategy as FollowStrategySettings;
			return this;
		}
		private long FollowBasedOnLikers()
		{
			var fetchMedias = MediaFetcherManager.Begin.Commit(FetchType.Media, _builder, _profile).FetchByTopic();
			var searchMedias = (IEnumerable<UserResponse<MediaDetail>>)fetchMedias.FetchedItems;
			searchMedias = searchMedias.Where(_ => _.Object.IsFollowing != null && _.Object.IsFollowing != true);
			var users_ = _builder.SearchInstagramMediaLikers(searchMedias.ElementAt(SecureRandom.Next(searchMedias.Count())).Object.MediaId);
			int index = 0;

			while (users_.Count>index)
			{
				var findNominatedUser = _builder.SearchInstagramFullUserDetail(users_.ElementAt(SecureRandom.Next(users_.Count)).UserId);
				if(findNominatedUser==null) return 0;
				double ratioff = (double)findNominatedUser.UserDetail.FollowerCount / findNominatedUser.UserDetail.FollowingCount;

				if (ratioff > 1.0 && findNominatedUser.UserDetail.MediaCount > 5)
				{
					return findNominatedUser.UserDetail.Pk;
				}
				Thread.Sleep(TimeSpan.FromSeconds(SecureRandom.Next(1, 4)));
				index++;
			}
			return 0;
		}
		private long FollowBasedOnTopic()
		{
			var fetchMedias = MediaFetcherManager.Begin.Commit(FetchType.Media, _builder, _profile).FetchByTopic();
			var searchMedias = (IEnumerable<UserResponse<MediaDetail>>)fetchMedias.FetchedItems;
			searchMedias = searchMedias.Where(_=>_.Object.IsFollowing!=null && _.Object.IsFollowing!=true);
			return searchMedias.ElementAt(SecureRandom.Next(searchMedias.Count())).UserId;
		}
		private long FollowBasedOnCommenters()
		{
			var fetchMedias = MediaFetcherManager.Begin.Commit(FetchType.Media, _builder, _profile).FetchByTopic();
			var searchMedias = (IEnumerable<UserResponse<MediaDetail>>)fetchMedias.FetchedItems;
			searchMedias = searchMedias.Where(_ =>
			{
				if (_ == null) return false;
				int commentCount = 0;
				int.TryParse(_.Object?.CommentCount, out commentCount);
				return _.Object.LikesCount > 5 && commentCount > 0 && !_.Object.IsCommentsDisabled;
			});
			var users_ = _builder.SearchInstagramMediaCommenters( 
				searchMedias.ElementAt(SecureRandom.Next(searchMedias.Count())).Object.MediaId,1);
			int index = 0;
			while (users_.Count > index)
			{
				var comment = users_.ElementAt(index).Object;
				if (comment.LikeCount > 1) 
				{
					var findNominatedUser = _builder.SearchInstagramFullUserDetail(users_.ElementAt(SecureRandom.Next(users_.Count)).UserId);
					double ratioff = (double)findNominatedUser.UserDetail.FollowerCount / findNominatedUser.UserDetail.FollowingCount;

					if (ratioff > 1.0 && findNominatedUser.UserDetail.MediaCount > 5)
					{
						return findNominatedUser.UserDetail.Pk;
					}
					Thread.Sleep(TimeSpan.FromSeconds(SecureRandom.Next(1, 4)));
				}
				index++;
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
