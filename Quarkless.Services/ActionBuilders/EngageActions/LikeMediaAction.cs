using Quarkless.Services.Factories;
using Quarkless.Services.Interfaces;
using Quarkless.Services.Interfaces.Actions;
using Quarkless.Services.StrategyBuilders;
using QuarklessContexts.Enums;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.ServicesModels.SearchModels;
using QuarklessContexts.Models.Timeline;
using QuarklessLogic.Handlers.RequestBuilder.Consts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Quarkless.Services.ActionBuilders.EngageActions
{
	public enum LikeActionType
	{
		Any = 0,
		LikeByTopic = 1,
		LikeUsersMediaByLikers = 2,
		LikeUsersMediaByLikersInDepth = 3,
		LikeFromUsersFeed = 4,
		LikeUsersMediaByCommenters = 5
	}
	public class LikeMediaAction : IActionCommit
	{
		private readonly IContentManager _builder;
		private readonly ProfileModel _profile;
		private LikeStrategySettings likeStrategySettings;
		public LikeMediaAction(IContentManager builder, ProfileModel profile)
		{
			_builder = builder;
			_profile = profile;
		}

		public IActionCommit IncludeStrategy(IStrategy strategy)
		{
			this.likeStrategySettings = strategy as LikeStrategySettings;
			return this;
		}
		private string LikeUserFeedMedia()
		{
			var medias = _builder.SearchUserFeedMediaDetailInstagram();
			var filteredMedia = medias.Where(_=>!_.Object.HasLikedBefore);
			return filteredMedia.ElementAt(SecureRandom.Next(filteredMedia.Count())).Object.MediaId;
		}
		private string LikeUsersMediaByTopic()
		{
			IEnumerable<UserResponse<MediaDetail>> searchMedias = (IEnumerable<UserResponse<MediaDetail>>) MediaFetcherManager.Begin.Commit(FetchType.Media,_builder,_profile).FetchByTopic();
			var filtered = searchMedias.Where(_=>!_.Object.HasLikedBefore);
			return filtered.ElementAt(SecureRandom.Next(filtered.Count())).Object.MediaId;
		}
		private string LikeUsersMediaLikers(bool inDepth = false)
		{
			IEnumerable<UserResponse<MediaDetail>> searchMedias = (IEnumerable<UserResponse<MediaDetail>>)MediaFetcherManager.Begin.Commit(FetchType.Media,_builder, _profile).FetchByTopic();

			if (inDepth) { 
				var users_ = _builder.SearchInstagramMediaLikers(searchMedias.ElementAt(SecureRandom.Next(searchMedias.Count())).Object.MediaId);
				int index = 0;
				while (users_.Count > index)
				{
					var findNominatedUser = _builder.SearchInstagramFullUserDetail(users_.ElementAt(SecureRandom.Next(users_.Count)).UserId);
					double ratioff = (double)findNominatedUser.UserDetail.FollowerCount / findNominatedUser.UserDetail.FollowingCount;

					if (ratioff > 1.0 && findNominatedUser.UserDetail.MediaCount > 5)
					{
						var resMedia = _builder.SearchUsersMediaDetailInstagram(findNominatedUser.UserDetail.Username,1);
						return resMedia.ElementAt(SecureRandom.Next(resMedia.Count())).Object.MediaId;
					}
					Thread.Sleep(TimeSpan.FromSeconds(SecureRandom.Next(1, 4)));
					index++;
				}
			}
			else
			{
				var resMedia = _builder.SearchUsersMediaDetailInstagram( 
					searchMedias.ElementAt(SecureRandom.Next(searchMedias.Count())).Username, 1);
				return resMedia.ElementAt(SecureRandom.Next(resMedia.Count())).Object.MediaId;
			}
			return null;
		}
		private string LikeUsersMediaCommenters()
		{
			IEnumerable<UserResponse<MediaDetail>> searchMedias = (IEnumerable<UserResponse<MediaDetail>>)MediaFetcherManager.Begin.Commit(FetchType.Media, _builder, _profile).FetchByTopic();
			var users_ = _builder.SearchInstagramMediaCommenters(searchMedias.ElementAt(SecureRandom.Next(searchMedias.Count())).Object.MediaId, 1);
			int index = 0;
			while (users_.Count > index)
			{
				var findNominatedUser = _builder.SearchInstagramFullUserDetail(users_.ElementAt(SecureRandom.Next(users_.Count)).UserId);
				double ratioff = (double)findNominatedUser.UserDetail.FollowerCount / findNominatedUser.UserDetail.FollowingCount;

				if (ratioff > 1.0 && findNominatedUser.UserDetail.MediaCount > 5)
				{
					var resMedia = _builder.SearchUsersMediaDetailInstagram(findNominatedUser.UserDetail.Username, 1);
					return resMedia.ElementAt(SecureRandom.Next(resMedia.Count())).Object.MediaId;
				}
				Thread.Sleep(TimeSpan.FromSeconds(SecureRandom.Next(1, 4)));
				index++;
			}
			return null;
		}

		public void Push(IActionOptions actionOptions)
		{
			LikeActionOptions likeActionOptions = actionOptions as LikeActionOptions;
			try
			{
				Console.WriteLine("Like Action Started");
				string nominatedMedia = string.Empty;
				LikeActionType likeActionTypeSelected = LikeActionType.LikeByTopic;
				if (likeActionOptions.LikeActionType == LikeActionType.Any)
				{
					List<Chance<LikeActionType>> likeActionsChances = new List<Chance<LikeActionType>>
					{
						new Chance<LikeActionType>{Object = LikeActionType.LikeByTopic, Probability = 0.10},
						new Chance<LikeActionType>{Object = LikeActionType.LikeUsersMediaByLikersInDepth, Probability = 0.30},
						new Chance<LikeActionType>{Object = LikeActionType.LikeFromUsersFeed, Probability = 0.30},
						new Chance<LikeActionType>{Object = LikeActionType.LikeUsersMediaByCommenters, Probability = 0.15},
						new Chance<LikeActionType>{Object = LikeActionType.LikeUsersMediaByLikers, Probability = 0.15}
					};
					likeActionTypeSelected = SecureRandom.ProbabilityRoll(likeActionsChances);
				}
				else
				{
					likeActionTypeSelected = likeActionOptions.LikeActionType;
				}
				switch (likeActionTypeSelected)
				{
					case LikeActionType.LikeByTopic:
						nominatedMedia = LikeUsersMediaByTopic();
						break;
					case LikeActionType.LikeFromUsersFeed:
						nominatedMedia = LikeUserFeedMedia();
						break;
					case LikeActionType.LikeUsersMediaByCommenters:
						nominatedMedia = LikeUsersMediaCommenters();
						break;
					case LikeActionType.LikeUsersMediaByLikers:
						nominatedMedia = LikeUsersMediaLikers(false);
						break;
					case LikeActionType.LikeUsersMediaByLikersInDepth:
						nominatedMedia = LikeUsersMediaLikers(true);
						break;
				}
				if (string.IsNullOrEmpty(nominatedMedia)) return;
				RestModel restModel = new RestModel
				{
					BaseUrl = string.Format(UrlConstants.LikeMedia, nominatedMedia),
					RequestType = RequestType.POST,
					JsonBody = null
				};
				_builder.AddToTimeline(restModel, likeActionOptions.ExecutionTime);
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
				return;
			}
		}
	}
}
