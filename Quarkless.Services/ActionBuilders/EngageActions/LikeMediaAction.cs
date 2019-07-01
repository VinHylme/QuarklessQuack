using MoreLinq;
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
		private UserStoreDetails user;
		private LikeStrategySettings likeStrategySettings;
		public LikeMediaAction(IContentManager builder, ProfileModel profile)
		{
			_builder = builder;
			_profile = profile;
		}

		public IActionCommit IncludeStrategy(IStrategySettings strategy)
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
			var fetchMedias = MediaFetcherManager.Begin.Commit(FetchType.Media, _builder, _profile).FetchByTopic();
			var searchMedias = (IEnumerable<UserResponse<MediaDetail>>)fetchMedias.FetchedItems;
			var filtered = searchMedias.Where(_=>!_.Object.HasLikedBefore);
			return filtered.ElementAt(SecureRandom.Next(filtered.Count())).Object.MediaId;
		}
		private string LikeUsersMediaLikers(bool inDepth = false)
		{
			var fetchMedias = MediaFetcherManager.Begin.Commit(FetchType.Media, _builder, _profile).FetchByTopic();
			var searchMedias = (IEnumerable<UserResponse<MediaDetail>>)fetchMedias.FetchedItems;
			if (inDepth) { 
				var users_ = _builder.SearchInstagramMediaLikers(searchMedias.ElementAt(SecureRandom.Next(searchMedias.Count())).Object.MediaId);
				int index = 0;
				while (users_.Count > index)
				{
					var findNominatedUser = _builder.SearchInstagramFullUserDetail(users_.ElementAt(SecureRandom.Next(users_.Count)).UserId);
					double ratioff = (double)findNominatedUser.UserDetail.FollowerCount / findNominatedUser.UserDetail.FollowingCount;

					if (ratioff > 1.0 && findNominatedUser.UserDetail.MediaCount > 5)
					{
						var resMedia = _builder.SearchUsersMediaDetailInstagram(findNominatedUser.UserDetail.UserName, 1);
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
			var fetchMedias = MediaFetcherManager.Begin.Commit(FetchType.Media, _builder, _profile).FetchByTopic();
			var searchMedias = (IEnumerable<UserResponse<MediaDetail>>)fetchMedias.FetchedItems;
			var users_ = _builder.SearchInstagramMediaCommenters(searchMedias.ElementAt(SecureRandom.Next(searchMedias.Count())).Object.MediaId, 1);
			int index = 0;
			while (users_.Count > index)
			{
				var findNominatedUser = _builder.SearchInstagramFullUserDetail(users_.ElementAt(SecureRandom.Next(users_.Count)).UserId);
				double ratioff = (double)findNominatedUser.UserDetail.FollowerCount / findNominatedUser.UserDetail.FollowingCount;

				if (ratioff > 1.0 && findNominatedUser.UserDetail.MediaCount > 5)
				{
					var resMedia = _builder.SearchUsersMediaDetailInstagram(findNominatedUser.UserDetail.UserName, 1);
					return resMedia.ElementAt(SecureRandom.Next(resMedia.Count())).Object.MediaId;
				}
				Thread.Sleep(TimeSpan.FromSeconds(SecureRandom.Next(1, 4)));
				index++;
			}
			return null;
		}
		public IEnumerable<TimelineEventModel> Push(IActionOptions actionOptions)
		{
			LikeActionOptions likeActionOptions = actionOptions as LikeActionOptions;
			if (likeStrategySettings == null && user==null) return null;
			try
			{
				Console.WriteLine("Like Action Started");
				if (likeStrategySettings.LikeStrategy == LikeStrategyType.Default)
				{
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
					if (string.IsNullOrEmpty(nominatedMedia)) return null;
					RestModel restModel = new RestModel
					{
						BaseUrl = string.Format(UrlConstants.LikeMedia, nominatedMedia),
						RequestType = RequestType.POST,
						User = user,
						JsonBody = null
					};
					return new List<TimelineEventModel>
					{
						new TimelineEventModel
						{
							ActionName = $"LikeMedia_{likeStrategySettings.LikeStrategy.ToString()}_{likeActionTypeSelected.ToString()}",
							Data = restModel,
							ExecutionTime = likeActionOptions.ExecutionTime
						}
					};
				}
				else if(likeStrategySettings.LikeStrategy == LikeStrategyType.TwoDollarCent)
				{
					var medias = (IEnumerable<UserResponse<MediaDetail>>) MediaFetcherManager.Begin.Commit(FetchType.Media,_builder,_profile).FetchByTopic(takeAmount:likeStrategySettings.NumberOfActions);
					var filtered = medias.Where(_ => !_.Object.HasLikedBefore);
					var groupByTopics = filtered.GroupBy(_=>_.Topic);
					int timerCounter = 0 ;
					List<TimelineEventModel> events = new List<TimelineEventModel>();
					foreach (var topic in groupByTopics)
					{
						for (int i = 0; i < likeStrategySettings.NumberOfActions; i++)
						{
							string nominatedMedia = topic.ElementAtOrDefault(i).Object.MediaId;
							if (nominatedMedia != null)
							{
								RestModel restModel = new RestModel
								{
									BaseUrl = string.Format(UrlConstants.LikeMedia, nominatedMedia),
									RequestType = RequestType.POST,
									JsonBody = null,
									User = user
								};
								events.Add(new TimelineEventModel
								{
									ActionName = $"LikeMedia{likeStrategySettings.LikeStrategy.ToString()}_{likeActionOptions.LikeActionType.ToString()}",
									Data = restModel,
									ExecutionTime = likeActionOptions.ExecutionTime.AddMinutes((likeStrategySettings.OffsetPerAction.TotalMinutes) * timerCounter++)
								});
							}
						}
					}
					return events;
				}

				return null;
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
