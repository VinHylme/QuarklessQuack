using InstagramApiSharp.Classes.Models;
using Quarkless.Services.Interfaces;
using Quarkless.Services.Interfaces.Actions;
using Quarkless.Services.StrategyBuilders;
using QuarklessContexts.Classes.Carriers;
using QuarklessContexts.Enums;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models;
using QuarklessContexts.Models.ServicesModels.HeartbeatModels;
using QuarklessContexts.Models.ServicesModels.SearchModels;
using QuarklessContexts.Models.Timeline;
using QuarklessLogic.Handlers.RequestBuilder.Consts;
using QuarklessLogic.ServicesLogic.HeartbeatLogic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Quarkless.Services.ActionBuilders.EngageActions
{
	public enum LikeCommentActionType
	{
		Any,
		ByTopic,
		ByCommenters,
		ByLikers,
		ByUserTarget,
		ByLocation,
		ByUserFeed
	}
	public class LikeCommentAction : IActionCommit
	{
		private readonly IHeartbeatLogic _heartbeatLogic;
		private UserStoreDetails user;
		private LikeStrategySettings likeStrategySettings;
		public LikeCommentAction(IContentManager contentManager, IHeartbeatLogic heartbeatLogic)
		{
			_heartbeatLogic = heartbeatLogic;
		}
		private long? CommentingByTopic()
		{
			var by = new By
			{
				ActionType = (int)ActionType.LikeComment,
				User = user.Profile.InstagramAccountId
			};
			var fetchComments = _heartbeatLogic.GetMetaData<List<UserResponse<InstaComment>>>(MetaDataType.FetchUsersViaPostCommented, user.Profile.Topics.TopicFriendlyName)
				.GetAwaiter().GetResult().Where(exclude => !exclude.SeenBy.Any(e => e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Count > 0);

			var meta_S = fetchComments as __Meta__<List<UserResponse<InstaComment>>>[] ?? fetchComments.ToArray();
			var select = meta_S.ElementAtOrDefault(SecureRandom.Next(meta_S.Count()));
			if (@select == null) return null;
			@select.SeenBy.Add(@by);
			_heartbeatLogic.UpdateMetaData(MetaDataType.FetchUsersViaPostCommented, user.Profile.Topics.TopicFriendlyName, @select).GetAwaiter().GetResult();
			var comment = @select.ObjectItem.FirstOrDefault();
			return comment?.Object.Pk;
		}
		private long? CommentingByCommenters()
		{
			var by = new By
			{
				ActionType = (int)ActionType.LikeComment,
				User = user.Profile.InstagramAccountId
			};
			var fetchComments = _heartbeatLogic.GetMetaData<List<UserResponse<InstaComment>>>(MetaDataType.FetchCommentsViaPostCommented, user.Profile.Topics.TopicFriendlyName)
			.GetAwaiter().GetResult().Where(exclude => !exclude.SeenBy.Any(e => e.User == by.User && e.ActionType == by.ActionType))
			.Where(s => s.ObjectItem.Count > 0);
			var meta_S = fetchComments as __Meta__<List<UserResponse<InstaComment>>>[] ?? fetchComments.ToArray();
			var select = meta_S.ElementAtOrDefault(SecureRandom.Next(meta_S.Count()));
			if (@select == null) return null;
			@select.SeenBy.Add(@by);
			_heartbeatLogic.UpdateMetaData(MetaDataType.FetchCommentsViaPostCommented, user.Profile.Topics.TopicFriendlyName, @select).GetAwaiter().GetResult();
			var comment = @select.ObjectItem.FirstOrDefault();
			return comment?.Object.Pk;
		}
		private long? CommentingByLikers()
		{
			var by = new By
			{
				ActionType = (int)ActionType.LikeComment,
				User = user.Profile.InstagramAccountId
			};
			var fetchComments = _heartbeatLogic.GetMetaData<List<UserResponse<InstaComment>>>(MetaDataType.FetchCommentsViaPostsLiked, user.Profile.Topics.TopicFriendlyName)
				.GetAwaiter().GetResult().Where(exclude => !exclude.SeenBy.Any(e => e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Count > 0);
			var meta_S = fetchComments as __Meta__<List<UserResponse<InstaComment>>>[] ?? fetchComments.ToArray();
			var select = meta_S.ElementAtOrDefault(SecureRandom.Next(meta_S.Count()));
			if (@select == null) return null;
			@select.SeenBy.Add(@by);
			_heartbeatLogic.UpdateMetaData<List<UserResponse<InstaComment>>>(MetaDataType.FetchCommentsViaPostsLiked, user.Profile.Topics.TopicFriendlyName, @select).GetAwaiter().GetResult();
			var comment = @select.ObjectItem.FirstOrDefault();
			return comment?.Object.Pk;
		}
		private long? CommentingByTarget()
		{
			var by = new By
			{
				ActionType = (int)ActionType.LikeComment,
				User = user.Profile.InstagramAccountId
			};
			var fetchComments = _heartbeatLogic.GetMetaData<List<UserResponse<InstaComment>>>(MetaDataType.FetchCommentsViaUserTargetList, user.Profile.Topics.TopicFriendlyName)
				.GetAwaiter().GetResult().Where(exclude => !exclude.SeenBy.Any(e => e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Count > 0);

			var meta_S = fetchComments as __Meta__<List<UserResponse<InstaComment>>>[] ?? fetchComments.ToArray();
			var select = meta_S.ElementAtOrDefault(SecureRandom.Next(meta_S.Count()));
			if (@select == null) return null;
			@select.SeenBy.Add(@by);
			_heartbeatLogic.UpdateMetaData(MetaDataType.FetchCommentsViaUserTargetList, user.Profile.Topics.TopicFriendlyName, @select).GetAwaiter().GetResult();
			var comment = @select.ObjectItem.FirstOrDefault();
			return comment?.Object.Pk;
		}
		private long? CommentingByLocation()
		{
			var by = new By
			{
				ActionType = (int)ActionType.LikeComment,
				User = user.Profile.InstagramAccountId
			};
			var fetchComments = _heartbeatLogic.GetMetaData<List<UserResponse<InstaComment>>>(MetaDataType.FetchCommentsViaLocationTargetList, user.Profile.Topics.TopicFriendlyName)
				.GetAwaiter().GetResult().Where(exclude => !exclude.SeenBy.Any(e => e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Count > 0);

			var meta_S = fetchComments as __Meta__<List<UserResponse<InstaComment>>>[] ?? fetchComments.ToArray();
			var select = meta_S.ElementAtOrDefault(SecureRandom.Next(meta_S.Count()));
			if (@select == null) return null;
			@select.SeenBy.Add(@by);
			_heartbeatLogic.UpdateMetaData(MetaDataType.FetchCommentsViaLocationTargetList, user.Profile.Topics.TopicFriendlyName, @select).GetAwaiter().GetResult();
			var comment = @select.ObjectItem.FirstOrDefault();
			return comment?.Object.Pk;
		}
		private long? CommentingByUserFeed()
		{
			var by = new By
			{
				ActionType = (int)ActionType.LikeComment,
				User = user.Profile.InstagramAccountId
			};
			var fetchComments = _heartbeatLogic.GetMetaData<List<UserResponse<InstaComment>>>(MetaDataType.FetchCommentsViaUserFeed, user.Profile.Topics.TopicFriendlyName)
				.GetAwaiter().GetResult().Where(exclude => !exclude.SeenBy.Any(e => e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Count > 0);

			var meta_S = fetchComments as __Meta__<List<UserResponse<InstaComment>>>[] ?? fetchComments.ToArray();
			var select = meta_S.ElementAtOrDefault(SecureRandom.Next(meta_S.Count()));
			if (@select == null) return null;
			@select.SeenBy.Add(@by);
			_heartbeatLogic.UpdateMetaData(MetaDataType.FetchCommentsViaUserFeed, user.Profile.Topics.TopicFriendlyName, @select).GetAwaiter().GetResult();
			var comment = @select.ObjectItem.FirstOrDefault();
			return comment?.Object.Pk;
		}


		public IActionCommit IncludeStrategy(IStrategySettings strategy)
		{
			likeStrategySettings = strategy as LikeStrategySettings;
			return this;
		}

		public IActionCommit IncludeUser(UserStoreDetails userStoreDetails)
		{
			user = userStoreDetails;
			return this;
		}

		public ResultCarrier<IEnumerable<TimelineEventModel>> Push(IActionOptions actionOptions)
		{
			Console.WriteLine($"Like Comment Action Started: {user.OAccountId}, {user.OInstagramAccountUsername}, {user.OInstagramAccountUser}");
			var results = new ResultCarrier<IEnumerable<TimelineEventModel>>();
			var likeActionOptions = actionOptions as LikeCommentActionOptions;

			if (likeStrategySettings == null && user == null)
			{
				results.IsSuccesful = false;
				results.Info = new ErrorResponse
				{
					Message = $"user is null, user: {user.OAccountId}, instaId: {user.OInstagramAccountUsername}",
					StatusCode = System.Net.HttpStatusCode.NotFound
				};
				return results;
			};

			try
			{
				if(likeStrategySettings != null && likeStrategySettings.LikeStrategy == LikeStrategyType.Default)
				{
					long? nominatedComment = null;
					var likeActionTypeSelected = LikeCommentActionType.ByTopic;
					if(likeActionOptions != null && likeActionOptions.LikeActionType == LikeCommentActionType.Any)
					{
						var likeActionsChances = new List<Chance<LikeCommentActionType>>();
						var focusLocal = user.Profile.AdditionalConfigurations.FocusLocalMore;

						if (user.Profile.UserTargetList != null) 
							if(user.Profile.UserTargetList.Count > 0)
								likeActionsChances.Add(new Chance<LikeCommentActionType> { Object = LikeCommentActionType.ByUserTarget, 
									Probability = focusLocal ? 0.25 : 0.10 });
							
						if(user.Profile.LocationTargetList != null)
							if(user.Profile.LocationTargetList.Count > 0)
								likeActionsChances.Add(new Chance<LikeCommentActionType> { Object = LikeCommentActionType.ByLocation, 
									Probability = focusLocal ? 0.60 : 0.10 });
							else
							{
								focusLocal = false;
							}
						else
						{
							focusLocal = false;
						}

						likeActionsChances.AddRange(new List<Chance<LikeCommentActionType>>
						{
							new Chance<LikeCommentActionType>{Object = LikeCommentActionType.ByTopic, 
								Probability = focusLocal ? 0.05 : 0.20},
							new Chance<LikeCommentActionType>{Object = LikeCommentActionType.ByUserFeed, 
								Probability =  focusLocal ? 0.10 : 0.20},
							new Chance<LikeCommentActionType>{Object = LikeCommentActionType.ByLikers, 
								Probability =  focusLocal ? 0.10 : 0.20},
							new Chance<LikeCommentActionType>{Object = LikeCommentActionType.ByCommenters, 
								Probability = focusLocal ? 0.10 : 0.20},
						});


						likeActionTypeSelected = SecureRandom.ProbabilityRoll(likeActionsChances);
					}
					else
					{
						likeActionTypeSelected = likeActionOptions.LikeActionType;
					}
					switch (likeActionTypeSelected)
					{
						case LikeCommentActionType.ByTopic:
							nominatedComment = CommentingByTopic();
							break;
						case LikeCommentActionType.ByUserFeed:
							nominatedComment = CommentingByUserFeed();
							break;
						case LikeCommentActionType.ByCommenters:
							nominatedComment = CommentingByCommenters();
							break;
						case LikeCommentActionType.ByLikers:
							nominatedComment = CommentingByLikers();
							break;
						case LikeCommentActionType.ByLocation:
							nominatedComment = CommentingByLocation();
							break;
						case LikeCommentActionType.ByUserTarget:
							nominatedComment = CommentingByTarget();
							break;
					}
					if (nominatedComment==null)
					{
						results.IsSuccesful = false;
						results.Info = new ErrorResponse
						{
							Message = $"could not find any good comment to like, user: {user.OAccountId}, instaId: {user.OInstagramAccountUsername}",
							StatusCode = System.Net.HttpStatusCode.NotFound
						};
						return results;
					}

					var restModel = new RestModel
					{
						BaseUrl = string.Format(UrlConstants.LikeComment, nominatedComment.ToString()),
						RequestType = RequestType.POST,
						User = user,
						JsonBody = null
					};
					results.IsSuccesful = true;
					results.Results = new List<TimelineEventModel>
					{
						new TimelineEventModel
						{
							ActionName = $"LikeComment_{likeStrategySettings.LikeStrategy.ToString()}_{likeActionTypeSelected.ToString()}",
							Data = restModel,
							ExecutionTime = likeActionOptions.ExecutionTime
						}
					};
					return results;
				}
				results.IsSuccesful = false;
				results.Info = new ErrorResponse
				{
					Message = $"strategy not implemented, user: {user.OAccountId}, instaId: {user.OInstagramAccountUsername}",
					StatusCode = System.Net.HttpStatusCode.Forbidden
				};
				return results;
			}
			catch(Exception ee)
			{
				results.IsSuccesful = false;
				results.Info = new ErrorResponse
				{
					Message = $"{ee.Message}, user: {user.OAccountId}, instaId: {user.OInstagramAccountUsername}",
					StatusCode = System.Net.HttpStatusCode.InternalServerError,
					Exception = ee
				};
				return results;
			}
		}
	}
}
