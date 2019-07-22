using InstagramApiSharp.Classes.Models;
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
		private readonly IContentManager _builder;
		private readonly IHeartbeatLogic _heartbeatLogic;
		private readonly ProfileModel _profile;
		private UserStoreDetails user;
		private LikeStrategySettings likeStrategySettings;
		public LikeCommentAction(IContentManager contentManager, IHeartbeatLogic heartbeatLogic, ProfileModel profileModel)
		{
			_builder = contentManager;
			_heartbeatLogic = heartbeatLogic;
			_profile = profileModel;
		}
		private long? CommentingByTopic()
		{
			By by = new By
			{
				ActionType = (int)ActionType.LikeComment,
				User = _profile.InstagramAccountId
			};
			var fetchComments = _heartbeatLogic.GetMetaData<List<UserResponse<InstaComment>>>(MetaDataType.FetchUsersViaPostCommented, _profile.Topics.TopicFriendlyName)
				.GetAwaiter().GetResult().Where(exclude => !exclude.SeenBy.Any(e => e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Count > 0);

			if (fetchComments != null)
			{
				var select = fetchComments.ElementAtOrDefault(SecureRandom.Next(fetchComments.Count()));
				if (select != null)
				{
					select.SeenBy.Add(by);
					_heartbeatLogic.UpdateMetaData<List<UserResponse<InstaComment>>>(MetaDataType.FetchUsersViaPostCommented, _profile.Topics.TopicFriendlyName, select).GetAwaiter().GetResult();
					var comment = select.ObjectItem.FirstOrDefault();
					if (comment != null)
					{
						return comment.Object.Pk;
					}
				}
			}
			return null;
		}
		private long? CommentingByCommenters()
		{
			By by = new By
			{
				ActionType = (int)ActionType.LikeComment,
				User = _profile.InstagramAccountId
			};
			var fetchComments = _heartbeatLogic.GetMetaData<List<UserResponse<InstaComment>>>(MetaDataType.FetchCommentsViaPostCommented, _profile.Topics.TopicFriendlyName)
				.GetAwaiter().GetResult().Where(exclude => !exclude.SeenBy.Any(e => e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Count > 0);

			if (fetchComments != null)
			{
				var select = fetchComments.ElementAtOrDefault(SecureRandom.Next(fetchComments.Count()));
				if (select != null)
				{
					select.SeenBy.Add(by);
					_heartbeatLogic.UpdateMetaData<List<UserResponse<InstaComment>>>(MetaDataType.FetchCommentsViaPostCommented, _profile.Topics.TopicFriendlyName, select).GetAwaiter().GetResult();
					var comment = select.ObjectItem.FirstOrDefault();
					if (comment != null)
					{
						return comment.Object.Pk;
					}
				}
			}
			return null;
		}
		private long? CommentingByLikers()
		{
			By by = new By
			{
				ActionType = (int)ActionType.LikeComment,
				User = _profile.InstagramAccountId
			};
			var fetchComments = _heartbeatLogic.GetMetaData<List<UserResponse<InstaComment>>>(MetaDataType.FetchCommentsViaPostsLiked, _profile.Topics.TopicFriendlyName)
				.GetAwaiter().GetResult().Where(exclude => !exclude.SeenBy.Any(e => e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Count > 0);

			if (fetchComments != null)
			{
				var select = fetchComments.ElementAtOrDefault(SecureRandom.Next(fetchComments.Count()));
				if (select != null)
				{
					select.SeenBy.Add(by);
					_heartbeatLogic.UpdateMetaData<List<UserResponse<InstaComment>>>(MetaDataType.FetchCommentsViaPostsLiked, _profile.Topics.TopicFriendlyName, select).GetAwaiter().GetResult();
					var comment = select.ObjectItem.FirstOrDefault();
					if (comment != null)
					{
						return comment.Object.Pk;
					}
				}
			}
			return null;
		}
		private long? CommentingByTarget()
		{
			By by = new By
			{
				ActionType = (int)ActionType.LikeComment,
				User = _profile.InstagramAccountId
			};
			var fetchComments = _heartbeatLogic.GetMetaData<List<UserResponse<InstaComment>>>(MetaDataType.FetchCommentsViaUserTargetList, _profile.Topics.TopicFriendlyName)
				.GetAwaiter().GetResult().Where(exclude => !exclude.SeenBy.Any(e => e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Count > 0);

			if (fetchComments != null)
			{
				var select = fetchComments.ElementAtOrDefault(SecureRandom.Next(fetchComments.Count()));
				if (select != null)
				{
					select.SeenBy.Add(by);
					_heartbeatLogic.UpdateMetaData<List<UserResponse<InstaComment>>>(MetaDataType.FetchCommentsViaUserTargetList, _profile.Topics.TopicFriendlyName, select).GetAwaiter().GetResult();
					var comment = select.ObjectItem.FirstOrDefault();
					if (comment != null)
					{
						return comment.Object.Pk;
					}
				}
			}
			return null;
		}
		private long? CommentingByLocation()
		{
			By by = new By
			{
				ActionType = (int)ActionType.LikeComment,
				User = _profile.InstagramAccountId
			};
			var fetchComments = _heartbeatLogic.GetMetaData<List<UserResponse<InstaComment>>>(MetaDataType.FetchCommentsViaLocationTargetList, _profile.Topics.TopicFriendlyName)
				.GetAwaiter().GetResult().Where(exclude => !exclude.SeenBy.Any(e => e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Count > 0);

			if (fetchComments != null)
			{
				var select = fetchComments.ElementAtOrDefault(SecureRandom.Next(fetchComments.Count()));
				if (select != null)
				{
					select.SeenBy.Add(by);
					_heartbeatLogic.UpdateMetaData<List<UserResponse<InstaComment>>>(MetaDataType.FetchCommentsViaLocationTargetList, _profile.Topics.TopicFriendlyName, select).GetAwaiter().GetResult();
					var comment = select.ObjectItem.FirstOrDefault();
					if (comment != null)
					{
						return comment.Object.Pk;
					}
				}
			}
			return null;
		}
		private long? CommentingByUserFeed()
		{
			By by = new By
			{
				ActionType = (int)ActionType.LikeComment,
				User = _profile.InstagramAccountId
			};
			var fetchComments = _heartbeatLogic.GetMetaData<List<UserResponse<InstaComment>>>(MetaDataType.FetchCommentsViaUserFeed, _profile.Topics.TopicFriendlyName)
				.GetAwaiter().GetResult().Where(exclude => !exclude.SeenBy.Any(e => e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Count > 0);

			if (fetchComments != null)
			{
				var select = fetchComments.ElementAtOrDefault(SecureRandom.Next(fetchComments.Count()));
				if (select != null)
				{
					select.SeenBy.Add(by);
					_heartbeatLogic.UpdateMetaData<List<UserResponse<InstaComment>>>(MetaDataType.FetchCommentsViaUserFeed, _profile.Topics.TopicFriendlyName, select).GetAwaiter().GetResult();
					var comment = select.ObjectItem.FirstOrDefault();
					if (comment != null)
					{
						return comment.Object.Pk;
					}
				}
			}
			return null;
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
			Console.WriteLine("Like Comment Action Started");
			ResultCarrier<IEnumerable<TimelineEventModel>> Results = new ResultCarrier<IEnumerable<TimelineEventModel>>();
			LikeCommentActionOptions likeActionOptions = actionOptions as LikeCommentActionOptions;

			if (likeStrategySettings == null && user == null)
			{
				Results.IsSuccesful = false;
				Results.Info = new ErrorResponse
				{
					Message = $"user is null, user: {user.OAccountId}, instaId: {user.OInstagramAccountUsername}",
					StatusCode = System.Net.HttpStatusCode.NotFound
				};
				return Results;
			};

			try
			{
				if(likeStrategySettings.LikeStrategy == LikeStrategyType.Default)
				{
					long? nominatedComment = null;
					LikeCommentActionType likeActionTypeSelected = LikeCommentActionType.ByTopic;
					if(likeActionOptions.LikeActionType == LikeCommentActionType.Any)
					{
						List<Chance<LikeCommentActionType>> likeActionsChances = new List<Chance<LikeCommentActionType>>
						{
							new Chance<LikeCommentActionType>{Object = LikeCommentActionType.ByTopic, Probability = 0.20},
							new Chance<LikeCommentActionType>{Object = LikeCommentActionType.ByUserFeed, Probability = 0.20},
							new Chance<LikeCommentActionType>{Object = LikeCommentActionType.ByLikers, Probability = 0.20},
							new Chance<LikeCommentActionType>{Object = LikeCommentActionType.ByCommenters, Probability = 0.20},
						};
						if (_profile.UserTargetList != null) 
							if(_profile.UserTargetList.Count > 0)
								likeActionsChances.Add(new Chance<LikeCommentActionType> { Object = LikeCommentActionType.ByUserTarget, Probability = 0.10 });
							
						if(_profile.LocationTargetList != null)
							if(_profile.LocationTargetList.Count > 0)
								likeActionsChances.Add(new Chance<LikeCommentActionType> { Object = LikeCommentActionType.ByLocation, Probability = 0.10 });
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
						Results.IsSuccesful = false;
						Results.Info = new ErrorResponse
						{
							Message = $"could not find any good comment to like, user: {user.OAccountId}, instaId: {user.OInstagramAccountUsername}",
							StatusCode = System.Net.HttpStatusCode.NotFound
						};
						return Results;
					}
					else
					{
						RestModel restModel = new RestModel
						{
							BaseUrl = string.Format(UrlConstants.LikeComment, nominatedComment.ToString()),
							RequestType = RequestType.POST,
							User = user,
							JsonBody = null
						};
						Results.IsSuccesful = true;
						Results.Results = new List<TimelineEventModel>
						{
							new TimelineEventModel
							{
								ActionName = $"LikeComment_{likeStrategySettings.LikeStrategy.ToString()}_{likeActionTypeSelected.ToString()}",
								Data = restModel,
								ExecutionTime = likeActionOptions.ExecutionTime
							}
						};
						return Results;
					}
				}
				Results.IsSuccesful = false;
				Results.Info = new ErrorResponse
				{
					Message = $"strategy not implemented, user: {user.OAccountId}, instaId: {user.OInstagramAccountUsername}",
					StatusCode = System.Net.HttpStatusCode.Forbidden
				};
				return Results;
			}
			catch(Exception ee)
			{
				Results.IsSuccesful = false;
				Results.Info = new ErrorResponse
				{
					Message = $"{ee.Message}, user: {user.OAccountId}, instaId: {user.OInstagramAccountUsername}",
					StatusCode = System.Net.HttpStatusCode.InternalServerError,
					Exception = ee
				};
				return Results;
			}
		}
	}
}
