using System;
using System.Collections.Generic;
using System.Linq;
using InstagramApiSharp.Classes.Models;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Common.Extensions;
using Quarkless.Models.Common.Models;
using Quarkless.Models.Common.Models.Carriers;
using Quarkless.Models.ContentInfo.Interfaces;
using Quarkless.Models.Heartbeat;
using Quarkless.Models.Heartbeat.Interfaces;
using Quarkless.Models.RequestBuilder.Interfaces;
using Quarkless.Models.SearchResponse;
using Quarkless.Models.Services.Automation.Enums.Actions.ActionType;
using Quarkless.Models.Services.Automation.Enums.Actions.StrategyType;
using Quarkless.Models.Services.Automation.Interfaces;
using Quarkless.Models.Services.Automation.Models.ActionOptions;
using Quarkless.Models.Services.Automation.Models.StrategySettings;
using Quarkless.Models.Storage.Interfaces;
using Quarkless.Models.Timeline;

namespace Quarkless.Logic.Services.Automation.Actions.EngageActions
{
	public class LikeCommentAction : IActionCommit
	{
		private readonly IHeartbeatLogic _heartbeatLogic;
		private readonly IUrlReader _urlReader;
		private UserStoreDetails user;
		private LikeStrategySettings likeStrategySettings;
		public LikeCommentAction(IContentInfoBuilder contentManager, IHeartbeatLogic heartbeatLogic, IUrlReader urlReader)
		{
			_heartbeatLogic = heartbeatLogic;
			_urlReader = urlReader;
		}
		private long? CommentingByTopic()
		{
			var by = new By
			{
				ActionType = (int)ActionType.LikeComment,
				User = user.Profile.InstagramAccountId
			};
			var fetchComments = _heartbeatLogic.GetMetaData<List<UserResponse<InstaComment>>>(new MetaDataFetchRequest
				{
					MetaDataType = MetaDataType.FetchUsersViaPostCommented,
					ProfileCategoryTopicId = user.Profile.ProfileTopic.Category._id,
					InstagramId = user.ShortInstagram.Id
				})
				.GetAwaiter().GetResult().Where(exclude => !exclude.SeenBy.Any(e => e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Count > 0);

			var metaS = fetchComments as Meta<List<UserResponse<InstaComment>>>[] ?? fetchComments.ToArray();
			var select = metaS.ElementAtOrDefault(SecureRandom.Next(metaS.Count()));
			if (select == null) return null;
			select.SeenBy.Add(by);
			_heartbeatLogic.UpdateMetaData(new MetaDataCommitRequest<List<UserResponse<InstaComment>>>
			{
				MetaDataType = MetaDataType.FetchUsersViaPostCommented,
				ProfileCategoryTopicId = user.Profile.ProfileTopic.Category._id,
				InstagramId = user.ShortInstagram.Id,
				Data = select
			}).GetAwaiter().GetResult();
			var comment = select.ObjectItem.FirstOrDefault();
			return comment?.Object.Pk;
		}
		private long? CommentingByCommenters()
		{
			var by = new By
			{
				ActionType = (int)ActionType.LikeComment,
				User = user.Profile.InstagramAccountId
			};
			var fetchComments = _heartbeatLogic.GetMetaData<List<UserResponse<InstaComment>>>
					(new MetaDataFetchRequest
					{
						MetaDataType = MetaDataType.FetchCommentsViaPostCommented, 
						ProfileCategoryTopicId = user.Profile.ProfileTopic.Category._id,
						InstagramId = user.ShortInstagram.Id
					})

			.GetAwaiter().GetResult().Where(exclude => !exclude.SeenBy.Any(e => e.User == by.User && e.ActionType == by.ActionType))
			.Where(s => s.ObjectItem.Count > 0);
			var metaS = fetchComments as Meta<List<UserResponse<InstaComment>>>[] ?? fetchComments.ToArray();
			var select = metaS.ElementAtOrDefault(SecureRandom.Next(metaS.Count()));
			if (select == null) return null;
			select.SeenBy.Add(by);
			_heartbeatLogic.UpdateMetaData(new MetaDataCommitRequest<List<UserResponse<InstaComment>>>
			{
				MetaDataType = MetaDataType.FetchCommentsViaPostCommented,
				ProfileCategoryTopicId = user.Profile.ProfileTopic.Category._id,
				InstagramId = user.ShortInstagram.Id,
				Data = select
			}).GetAwaiter().GetResult();
			var comment = select.ObjectItem.FirstOrDefault();
			return comment?.Object.Pk;
		}
		private long? CommentingByLikers()
		{
			var by = new By
			{
				ActionType = (int)ActionType.LikeComment,
				User = user.Profile.InstagramAccountId
			};
			var fetchComments = _heartbeatLogic.GetMetaData<List<UserResponse<InstaComment>>>
					(new MetaDataFetchRequest
					{
						MetaDataType = MetaDataType.FetchCommentsViaPostsLiked,
						ProfileCategoryTopicId = user.Profile.ProfileTopic.Category._id,
						InstagramId = user.ShortInstagram.Id
					})
				.GetAwaiter().GetResult().Where(exclude => !exclude.SeenBy.Any(e => e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Count > 0);
			var metaS = fetchComments as Meta<List<UserResponse<InstaComment>>>[] ?? fetchComments.ToArray();
			var select = metaS.ElementAtOrDefault(SecureRandom.Next(metaS.Count()));
			if (select == null) return null;
			select.SeenBy.Add(by);
			_heartbeatLogic.UpdateMetaData<List<UserResponse<InstaComment>>>
				(new MetaDataCommitRequest<List<UserResponse<InstaComment>>>()
			{
				MetaDataType = MetaDataType.FetchCommentsViaPostsLiked,
				ProfileCategoryTopicId = user.Profile.ProfileTopic.Category._id,
				InstagramId = user.ShortInstagram.Id,
				Data = select
			}).GetAwaiter().GetResult();
			var comment = select.ObjectItem.FirstOrDefault();
			return comment?.Object.Pk;
		}
		private long? CommentingByTarget()
		{
			var by = new By
			{
				ActionType = (int)ActionType.LikeComment,
				User = user.Profile.InstagramAccountId
			};
			var fetchComments = _heartbeatLogic.GetMetaData<List<UserResponse<InstaComment>>>
					(new MetaDataFetchRequest
					{
						MetaDataType = MetaDataType.FetchCommentsViaUserTargetList,
						ProfileCategoryTopicId = user.Profile.ProfileTopic.Category._id,
						InstagramId = user.ShortInstagram.Id
					})
				.GetAwaiter().GetResult().Where(exclude => !exclude.SeenBy.Any(e => e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Count > 0);

			var metaS = fetchComments as Meta<List<UserResponse<InstaComment>>>[] ?? fetchComments.ToArray();
			var select = metaS.ElementAtOrDefault(SecureRandom.Next(metaS.Count()));
			if (select == null) return null;
			select.SeenBy.Add(by);
			_heartbeatLogic.UpdateMetaData(
				new MetaDataCommitRequest<List<UserResponse<InstaComment>>>
				{
					MetaDataType = MetaDataType.FetchCommentsViaUserTargetList,
					ProfileCategoryTopicId = user.Profile.ProfileTopic.Category._id,
					InstagramId = user.ShortInstagram.Id,
					Data = select
				}).GetAwaiter().GetResult();
			var comment = select.ObjectItem.FirstOrDefault();
			return comment?.Object.Pk;
		}
		private long? CommentingByLocation()
		{
			var by = new By
			{
				ActionType = (int)ActionType.LikeComment,
				User = user.Profile.InstagramAccountId
			};
			var fetchComments = _heartbeatLogic.GetMetaData<List<UserResponse<InstaComment>>>
					(new MetaDataFetchRequest
					{
						MetaDataType = MetaDataType.FetchCommentsViaLocationTargetList,
						ProfileCategoryTopicId = user.Profile.ProfileTopic.Category._id,
						InstagramId = user.ShortInstagram.Id
					})
				.GetAwaiter().GetResult().Where(exclude => !exclude.SeenBy.Any(e => e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Count > 0);

			var metaS = fetchComments as Meta<List<UserResponse<InstaComment>>>[] ?? fetchComments.ToArray();
			var select = metaS.ElementAtOrDefault(SecureRandom.Next(metaS.Count()));
			if (select == null) return null;
			select.SeenBy.Add(by);
			_heartbeatLogic.UpdateMetaData(new MetaDataCommitRequest<List<UserResponse<InstaComment>>>
			{
				MetaDataType = MetaDataType.FetchCommentsViaLocationTargetList,
				ProfileCategoryTopicId = user.Profile.ProfileTopic.Category._id,
				InstagramId = user.ShortInstagram.Id,
				Data = select
			}).GetAwaiter().GetResult();
			var comment = select.ObjectItem.FirstOrDefault();
			return comment?.Object.Pk;
		}
		private long? CommentingByUserFeed()
		{
			var by = new By
			{
				ActionType = (int)ActionType.LikeComment,
				User = user.Profile.InstagramAccountId
			};
			var fetchComments = _heartbeatLogic.GetMetaData<List<UserResponse<InstaComment>>>(
					new MetaDataFetchRequest
					{
						MetaDataType = MetaDataType.FetchCommentsViaUserFeed,
						ProfileCategoryTopicId = user.Profile.ProfileTopic.Category._id,
						InstagramId = user.ShortInstagram.Id
					})
				.GetAwaiter().GetResult().Where(exclude => !exclude.SeenBy.Any(e => e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Count > 0);

			var metaS = fetchComments as Meta<List<UserResponse<InstaComment>>>[] ?? fetchComments.ToArray();
			var select = metaS.ElementAtOrDefault(SecureRandom.Next(metaS.Count()));
			if (select == null) return null;
			select.SeenBy.Add(by);
			_heartbeatLogic.UpdateMetaData(new MetaDataCommitRequest<List<UserResponse<InstaComment>>>
			{
				MetaDataType = MetaDataType.FetchCommentsViaUserFeed,
				ProfileCategoryTopicId = user.Profile.ProfileTopic.Category._id,
				InstagramId = user.ShortInstagram.Id,
				Data = select
			}).GetAwaiter().GetResult();
			var comment = select.ObjectItem.FirstOrDefault();
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

		public IActionCommit IncludeStorage(IStorage storage)
		{
			throw new NotImplementedException();
		}

		public ResultCarrier<IEnumerable<TimelineEventModel>> Push(IActionOptions actionOptions)
		{
			Console.WriteLine($"Like Comment Action Started: {user.OAccountId}, {user.OInstagramAccountUsername}, {user.OInstagramAccountUser}");
			var results = new ResultCarrier<IEnumerable<TimelineEventModel>>();
			var likeActionOptions = actionOptions as LikeCommentActionOptions;

			if (likeStrategySettings == null && user == null)
			{
				results.IsSuccessful = false;
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
						results.IsSuccessful = false;
						results.Info = new ErrorResponse
						{
							Message = $"could not find any good comment to like, user: {user.OAccountId}, instaId: {user.OInstagramAccountUsername}",
							StatusCode = System.Net.HttpStatusCode.NotFound
						};
						return results;
					}

					var restModel = new RestModel
					{
						BaseUrl = string.Format(_urlReader.LikeComment, nominatedComment.ToString()),
						RequestType = RequestType.Post,
						User = user,
						JsonBody = null
					};
					results.IsSuccessful = true;
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
				results.IsSuccessful = false;
				results.Info = new ErrorResponse
				{
					Message = $"strategy not implemented, user: {user.OAccountId}, instaId: {user.OInstagramAccountUsername}",
					StatusCode = System.Net.HttpStatusCode.Forbidden
				};
				return results;
			}
			catch(Exception ee)
			{
				results.IsSuccessful = false;
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
