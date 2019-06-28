using Newtonsoft.Json;
using Quarkless.Services.Factories;
using Quarkless.Services.Interfaces;
using Quarkless.Services.Interfaces.Actions;
using Quarkless.Services.StrategyBuilders;
using QuarklessContexts.Enums;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models;
using QuarklessContexts.Models.Profiles;
using QuarklessContexts.Models.Requests;
using QuarklessContexts.Models.ServicesModels.DatabaseModels;
using QuarklessContexts.Models.ServicesModels.SearchModels;
using QuarklessContexts.Models.Timeline;
using QuarklessLogic.Handlers.RequestBuilder.Consts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Quarkless.Services.ActionBuilders.EngageActions
{
	public enum CommentingActionType
	{
		Any,
		CommentingViaLikersPosts, 
		CommentingViaLikersPostsDepth,
		CommentingViaTopic,
		CommentingViaTopicDepth,
		CommentingUserReply,
		CommentingPostReply
	}
	public class HolderComment
	{
		public TopicsModel Topic {get;set; }
		public string MediaId {get;set; }
	}
	public class CreateCommentAction : IActionCommit
	{
		private readonly IContentManager _builder;
		private readonly ProfileModel _profile;
		private readonly UserStore _user;
		private CommentingStrategySettings commentingStrategySettings;
		public CreateCommentAction(IContentManager contentManager, ProfileModel profile, UserStore user)
		{
			_builder = contentManager;
			_profile = profile;
			_user = user;
		}
		private HolderComment CommentingByTopic(bool inDepth = false)
		{
			var fetchMedias = MediaFetcherManager.Begin.Commit(FetchType.Media, _builder, _profile).FetchByTopic();
			var searchMedias = (IEnumerable<UserResponse<MediaDetail>>)fetchMedias.FetchedItems;
			if (inDepth)
			{
				int index = 0;
				while (searchMedias.Count() > index)
				{
					var findNominatedUser = _builder.SearchInstagramFullUserDetail(searchMedias.ElementAt(SecureRandom.Next(searchMedias.Count())).UserId);
					double ratioff = (double)findNominatedUser.UserDetail.FollowerCount / findNominatedUser.UserDetail.FollowingCount;

					if (ratioff > 1.0 && findNominatedUser.UserDetail.MediaCount > 5)
					{
						var resMedia = _builder.SearchUsersMediaDetailInstagram(findNominatedUser.UserDetail.Username, 1);
						var selected = resMedia.ElementAt(SecureRandom.Next(resMedia.Count()));
						return new HolderComment{ Topic =  fetchMedias.SelectedTopic, MediaId = selected.Object.MediaId };
					}
					Thread.Sleep(TimeSpan.FromSeconds(SecureRandom.Next(1, 4)));
					index++;
				}
			}
			else
			{
				var selected = searchMedias.ElementAt(SecureRandom.Next(searchMedias.Count()));
				return new HolderComment { Topic = fetchMedias.SelectedTopic, MediaId = selected.Object.MediaId };
			}
			return null;
		}
		private HolderComment CommentingByLikers(bool inDepth = false)
		{
			var fetchMedias = MediaFetcherManager.Begin.Commit(FetchType.Media, _builder, _profile).FetchByTopic();
			var searchMedias = (IEnumerable<UserResponse<MediaDetail>>) fetchMedias.FetchedItems;
			if (inDepth)
			{
				var users_ = _builder.SearchInstagramMediaLikers(searchMedias.ElementAt(SecureRandom.Next(searchMedias.Count())).Object.MediaId);
				int index = 0;
				while (users_.Count > index)
				{
					var findNominatedUser = _builder.SearchInstagramFullUserDetail(users_.ElementAt(SecureRandom.Next(users_.Count)).UserId);
					double ratioff = (double)findNominatedUser.UserDetail.FollowerCount / findNominatedUser.UserDetail.FollowingCount;

					if (ratioff > 1.0 && findNominatedUser.UserDetail.MediaCount > 5)
					{
						var resMedia = _builder.SearchUsersMediaDetailInstagram(findNominatedUser.UserDetail.Username, 1);
						var selected = resMedia.ElementAt(SecureRandom.Next(resMedia.Count()));
						return new HolderComment { Topic = fetchMedias.SelectedTopic, MediaId = selected.Object.MediaId };
					}
					Thread.Sleep(TimeSpan.FromSeconds(SecureRandom.Next(1, 4)));
					index++;
				}
			}
			else
			{
				var resMedia = _builder.SearchUsersMediaDetailInstagram(
					searchMedias.ElementAt(SecureRandom.Next(searchMedias.Count())).Username, 1);
				var selected = resMedia.ElementAt(SecureRandom.Next(resMedia.Count()));
				return new HolderComment { Topic = fetchMedias.SelectedTopic, MediaId = selected.Object.MediaId };
			}
			return null;
		}
		public IActionCommit IncludeStrategy(IStrategySettings strategy)
		{
			commentingStrategySettings = strategy as CommentingStrategySettings;
			return this;
		}
		public IEnumerable<TimelineEventModel> Push(IActionOptions actionOptions)
		{
			CommentingActionOptions commentingActionOptions = actionOptions as CommentingActionOptions;

			if(commentingActionOptions==null) return null ;
			try
			{
				if(commentingStrategySettings.CommentingStrategy == CommentingStrategy.Default)
				{
					HolderComment nominatedMedia = new HolderComment();
					CommentingActionType commentingActionSelected = CommentingActionType.CommentingViaTopic;
					if (commentingActionOptions.CommentingActionType == CommentingActionType.Any)
					{
						List<Chance<CommentingActionType>> commentingActionChances = new List<Chance<CommentingActionType>>
						{
							new Chance<CommentingActionType>{Object = CommentingActionType.CommentingViaTopic, Probability = 0.35},
							new Chance<CommentingActionType>{Object = CommentingActionType.CommentingViaTopicDepth, Probability = 0.15},
							new Chance<CommentingActionType>{Object = CommentingActionType.CommentingViaLikersPosts, Probability = 0.35},
							new Chance<CommentingActionType>{Object = CommentingActionType.CommentingViaLikersPostsDepth, Probability = 0.15},
						};
						commentingActionSelected = SecureRandom.ProbabilityRoll(commentingActionChances);
					}
					else
					{
						commentingActionSelected = commentingActionOptions.CommentingActionType;
					}

					switch (commentingActionSelected)
					{
						case CommentingActionType.CommentingViaTopic:
							nominatedMedia = CommentingByTopic();
							break;
						case CommentingActionType.CommentingViaTopicDepth:
							nominatedMedia = CommentingByTopic(true);
							break;
						case CommentingActionType.CommentingViaLikersPosts:
							nominatedMedia = CommentingByLikers();
							break;
						case CommentingActionType.CommentingViaLikersPostsDepth:
							nominatedMedia = CommentingByLikers(true);
							break;
					}
					if(string.IsNullOrEmpty(nominatedMedia.MediaId)) return null;
					CreateCommentRequest createComment = new CreateCommentRequest
					{
						Text = _builder.GenerateComment(nominatedMedia.Topic, _profile.Language)
					};
					RestModel restModel = new RestModel
					{
						BaseUrl = string.Format(UrlConstants.CreateComment, nominatedMedia.MediaId),
						RequestType = RequestType.POST,
						JsonBody = JsonConvert.SerializeObject(createComment),
						User = _user
					};

					return new List<TimelineEventModel>
					{
						new TimelineEventModel{
							ActionName = $"Comment_{commentingStrategySettings.CommentingStrategy.ToString()}_{commentingActionSelected.ToString()}",
							Data = restModel,
							ExecutionTime = commentingActionOptions.ExecutionTime 
						} 
					};
				}
				else if(commentingStrategySettings.CommentingStrategy == CommentingStrategy.TopNth)
				{
					var fetchMedias = MediaFetcherManager.Begin.Commit(FetchType.Media, _builder, _profile).FetchByTopic(takeAmount:commentingStrategySettings.NumberOfActions);
					var searchMedias = (IEnumerable<UserResponse<MediaDetail>>)fetchMedias.FetchedItems;

					var groupByTopics = searchMedias.GroupBy(_ => _.Topic);
					int timerCounter = 0;
					List<TimelineEventModel> events = new List<TimelineEventModel>();
					foreach (var topic in groupByTopics)
					{
						for (int i = 0; i < commentingStrategySettings.NumberOfActions; i++)
						{
							string nominatedMedia = topic.ElementAtOrDefault(i).Object.MediaId;
							if (nominatedMedia != null)
							{
								CreateCommentRequest createComment = new CreateCommentRequest
								{
									Text = _builder.GenerateComment(fetchMedias.SelectedTopic, _profile.Language)
								};

								RestModel restModel = new RestModel
								{
									BaseUrl = string.Format(UrlConstants.CreateComment, nominatedMedia),
									RequestType = RequestType.POST,
									JsonBody = JsonConvert.SerializeObject(createComment),
									User = _user
								};
								events.Add(new TimelineEventModel
								{
									ActionName = $"Comment_{commentingStrategySettings.CommentingStrategy.ToString()}_{commentingActionOptions.CommentingActionType.ToString()}",
									Data = restModel,
									ExecutionTime = commentingActionOptions.ExecutionTime.AddMinutes((commentingStrategySettings.OffsetPerAction.TotalMinutes) * timerCounter++)
								});
							}
						}
					}

					return events;
				}

				return null;
			}
			catch(Exception ee)
			{
				Console.WriteLine(ee.Message);
				return null;
			}
		}
	}
}
