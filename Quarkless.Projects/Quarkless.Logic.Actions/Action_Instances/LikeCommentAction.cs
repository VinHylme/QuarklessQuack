using Quarkless.Models.Heartbeat.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InstagramApiSharp.Classes.Models;
using Quarkless.Models.Actions;
using Quarkless.Models.Actions.Enums.ActionTypes;
using Quarkless.Models.Actions.Enums.StrategyType;
using Quarkless.Models.Actions.Factory.Action_Options;
using Quarkless.Models.Actions.Interfaces;
using Quarkless.Models.Actions.Models;
using Quarkless.Models.Comments;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Common.Extensions;
using Quarkless.Models.Common.Models;
using Quarkless.Models.Common.Models.Carriers;
using Quarkless.Models.ContentInfo.Interfaces;
using Quarkless.Models.Heartbeat;
using Quarkless.Models.SearchResponse;

namespace Quarkless.Logic.Actions.Action_Instances
{
	internal class LikeCommentAction : IActionCommit
	{
		private readonly IContentInfoBuilder _contentInfoBuilder;
		private readonly IHeartbeatLogic _heartbeatLogic;
		private readonly UserStoreDetails _user;
		private LikeCommentActionOptions _actionOptions;
		internal LikeCommentAction(UserStoreDetails userStoreDetails, IContentInfoBuilder contentInfoBuilder,
			IHeartbeatLogic heartbeatLogic)
		{
			_user = userStoreDetails;
			_contentInfoBuilder = contentInfoBuilder;
			_heartbeatLogic = heartbeatLogic;
			_actionOptions = new LikeCommentActionOptions();
		}

		private async Task<long?> CommentingByTopic()
		{
			var by = new By
			{
				ActionType = (int)ActionType.LikeComment,
				User = _user.Profile.InstagramAccountId
			};

			var fetchComments = (await _heartbeatLogic.GetMetaData<List<UserResponse<InstaComment>>>(new MetaDataFetchRequest
			{
				MetaDataType = MetaDataType.FetchUsersViaPostCommented,
				ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
				InstagramId = _user.ShortInstagram.Id
			}))?.Where(exclude => !exclude.SeenBy.Any(e => e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Count > 0).ToList();

			var select = fetchComments?.ElementAtOrDefault(SecureRandom.Next(fetchComments.Count));
			
			if (select == null) return null;
			select.SeenBy.Add(by);

			await _heartbeatLogic.UpdateMetaData(new MetaDataCommitRequest<List<UserResponse<InstaComment>>>
			{
				MetaDataType = MetaDataType.FetchUsersViaPostCommented,
				ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
				InstagramId = _user.ShortInstagram.Id,
				Data = select
			});

			var comment = select.ObjectItem.FirstOrDefault();
			return comment?.Object?.Pk;
		}
		private async Task<long?> CommentingByCommenters()
		{
			var by = new By
			{
				ActionType = (int)ActionType.LikeComment,
				User = _user.Profile.InstagramAccountId
			};

			var fetchComments = (await _heartbeatLogic.GetMetaData<List<UserResponse<InstaComment>>>
					(new MetaDataFetchRequest
					{
						MetaDataType = MetaDataType.FetchCommentsViaPostCommented,
						ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
						InstagramId = _user.ShortInstagram.Id
					}))?.Where(exclude => !exclude.SeenBy.Any(e => e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Count > 0).ToList();

			var select = fetchComments?.ElementAtOrDefault(SecureRandom.Next(fetchComments.Count));
			
			if (select == null) return null;
			select.SeenBy.Add(by);

			await _heartbeatLogic.UpdateMetaData(new MetaDataCommitRequest<List<UserResponse<InstaComment>>>
			{
				MetaDataType = MetaDataType.FetchCommentsViaPostCommented,
				ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
				InstagramId = _user.ShortInstagram.Id,
				Data = select
			});

			var comment = select.ObjectItem.FirstOrDefault();
			return comment?.Object?.Pk;
		}
		private async Task<long?> CommentingByLikers()
		{
			var by = new By
			{
				ActionType = (int)ActionType.LikeComment,
				User = _user.Profile.InstagramAccountId
			};

			var fetchComments = (await _heartbeatLogic.GetMetaData<List<UserResponse<InstaComment>>>
					(new MetaDataFetchRequest
					{
						MetaDataType = MetaDataType.FetchCommentsViaPostsLiked,
						ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
						InstagramId = _user.ShortInstagram.Id
					}))?.Where(exclude => !exclude.SeenBy.Any(e => e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Count > 0).ToList();

			var select = fetchComments?.ElementAtOrDefault(SecureRandom.Next(fetchComments.Count));
			
			if (select == null) return null;

			select.SeenBy.Add(by);

			await _heartbeatLogic.UpdateMetaData(new MetaDataCommitRequest<List<UserResponse<InstaComment>>>()
			{
				MetaDataType = MetaDataType.FetchCommentsViaPostsLiked,
				ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
				InstagramId = _user.ShortInstagram.Id,
				Data = select
			});

			var comment = select.ObjectItem.FirstOrDefault();
			return comment?.Object?.Pk;
		}
		private async Task<long?> CommentingByTarget()
		{
			var by = new By
			{
				ActionType = (int)ActionType.LikeComment,
				User = _user.Profile.InstagramAccountId
			};

			var fetchComments = (await _heartbeatLogic.GetMetaData<List<UserResponse<InstaComment>>>
					(new MetaDataFetchRequest
					{
						MetaDataType = MetaDataType.FetchCommentsViaUserTargetList,
						ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
						InstagramId = _user.ShortInstagram.Id
					}))?.Where(exclude => !exclude.SeenBy.Any(e => e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Count > 0).ToList();

			var select = fetchComments?.ElementAtOrDefault(SecureRandom.Next(fetchComments.Count));

			if (select == null) return null;
			select.SeenBy.Add(by);

			await _heartbeatLogic.UpdateMetaData(
				new MetaDataCommitRequest<List<UserResponse<InstaComment>>>
				{
					MetaDataType = MetaDataType.FetchCommentsViaUserTargetList,
					ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
					InstagramId = _user.ShortInstagram.Id,
					Data = select
				});

			var comment = select.ObjectItem.FirstOrDefault();
			return comment?.Object?.Pk;
		}
		private async Task<long?> CommentingByLocation()
		{
			var by = new By
			{
				ActionType = (int)ActionType.LikeComment,
				User = _user.Profile.InstagramAccountId
			};

			var fetchComments = (await _heartbeatLogic.GetMetaData<List<UserResponse<InstaComment>>>
					(new MetaDataFetchRequest
					{
						MetaDataType = MetaDataType.FetchCommentsViaLocationTargetList,
						ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
						InstagramId = _user.ShortInstagram.Id
					}))?.Where(exclude => !exclude.SeenBy.Any(e => e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Count > 0).ToList();

			var select = fetchComments?.ElementAtOrDefault(SecureRandom.Next(fetchComments.Count));

			if (select == null) return null;
			select.SeenBy.Add(by);

			await _heartbeatLogic.UpdateMetaData(new MetaDataCommitRequest<List<UserResponse<InstaComment>>>
			{
				MetaDataType = MetaDataType.FetchCommentsViaLocationTargetList,
				ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
				InstagramId = _user.ShortInstagram.Id,
				Data = select
			});

			var comment = select.ObjectItem.FirstOrDefault();
			return comment?.Object?.Pk;
		}
		private async Task<long?> CommentingByUserFeed()
		{
			var by = new By
			{
				ActionType = (int)ActionType.LikeComment,
				User = _user.Profile.InstagramAccountId
			};
			var fetchComments = (await _heartbeatLogic.GetMetaData<List<UserResponse<InstaComment>>>(
					new MetaDataFetchRequest
					{
						MetaDataType = MetaDataType.FetchCommentsViaUserFeed,
						ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
						InstagramId = _user.ShortInstagram.Id
					}))?.Where(exclude => !exclude.SeenBy.Any(e => e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Count > 0).ToList();

			var select = fetchComments?.ElementAtOrDefault(SecureRandom.Next(fetchComments.Count));

			if (select == null) return null;
			select.SeenBy.Add(by);

			await _heartbeatLogic.UpdateMetaData(new MetaDataCommitRequest<List<UserResponse<InstaComment>>>
			{
				MetaDataType = MetaDataType.FetchCommentsViaUserFeed,
				ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
				InstagramId = _user.ShortInstagram.Id,
				Data = select
			});

			var comment = select.ObjectItem.FirstOrDefault();
			return comment?.Object?.Pk;
		}

		public async Task<ResultCarrier<EventActionModel>> PushAsync(DateTimeOffset executionTime)
		{
			Console.WriteLine($"Like Comment Action Started: {_user.AccountId}, {_user.InstagramAccountUsername}, {_user.InstagramAccountUser}");
			var results = new ResultCarrier<EventActionModel>();

			try
			{
				switch (_actionOptions.StrategySettings.StrategyType)
				{
					case LikeStrategyType.Default:
						long? nominatedComment = null;
						LikeCommentActionType likeActionTypeSelected;

						if (_actionOptions.LikeActionType == LikeCommentActionType.Any)
						{
							var likeActionsChances = new List<Chance<LikeCommentActionType>>();
							var focusLocal = _user.Profile.AdditionalConfigurations.FocusLocalMore;

							if (_user.Profile.UserTargetList != null)
								if (_user.Profile.UserTargetList.Count > 0)
									likeActionsChances.Add(new Chance<LikeCommentActionType>
									{
										Object = LikeCommentActionType.ByUserTarget,
										Probability = focusLocal ? 0.25 : 0.10
									});

							if (_user.Profile.LocationTargetList != null)
								if (_user.Profile.LocationTargetList.Count > 0)
									likeActionsChances.Add(new Chance<LikeCommentActionType>
									{
										Object = LikeCommentActionType.ByLocation,
										Probability = focusLocal ? 0.60 : 0.10
									});
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
							likeActionTypeSelected = _actionOptions.LikeActionType;
						}
						switch (likeActionTypeSelected)
						{
							case LikeCommentActionType.ByTopic:
								nominatedComment = await CommentingByTopic();
								break;
							case LikeCommentActionType.ByUserFeed:
								nominatedComment = await CommentingByUserFeed();
								break;
							case LikeCommentActionType.ByCommenters:
								nominatedComment = await CommentingByCommenters();
								break;
							case LikeCommentActionType.ByLikers:
								nominatedComment = await CommentingByLikers();
								break;
							case LikeCommentActionType.ByLocation:
								nominatedComment = await CommentingByLocation();
								break;
							case LikeCommentActionType.ByUserTarget:
								nominatedComment = await CommentingByTarget();
								break;
						}
						if (nominatedComment == null)
						{
							results.IsSuccessful = false;
							results.Info = new ErrorResponse
							{
								Message = $"could not find any good comment to like, user: {_user.AccountId}, instaId: {_user.InstagramAccountUsername}",
								StatusCode = System.Net.HttpStatusCode.NotFound
							};
							return results;
						}
						var @event = new EventActionModel($"LikeComment_{_actionOptions.StrategySettings.StrategyType.ToString()}_{likeActionTypeSelected.ToString()}")
						{
							ActionType = ActionType.LikeComment,
							User = new UserStore
							{
								AccountId = _user.AccountId,
								InstagramAccountUsername = _user.InstagramAccountUsername,
								InstagramAccountUser = _user.InstagramAccountUser
							}
						};
						var request = new LikeCommentRequest
						{
							CommentId = nominatedComment.Value
						};
						@event.DataObjects.Add(new EventBody(request, request.GetType(), executionTime));
						results.IsSuccessful = true;
						results.Results = @event;
						return results;
					case LikeStrategyType.TwoDollarCent:
						throw new NotImplementedException();
					default:throw new Exception("Invalid Strategy Type");
				}
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
				results.IsSuccessful = false;
				results.Info = new ErrorResponse
				{
					Exception = err,
					Message = err.Message
				};
				return results;
			}
			finally
			{
				Console.WriteLine($"Like Comment Action Ended: {_user.AccountId}, {_user.InstagramAccountUsername}, {_user.InstagramAccountUser}");
			}
		}

		public IActionCommit ModifyOptions(IActionOptions newOptions)
		{
			try
			{
				_actionOptions = newOptions as LikeCommentActionOptions;
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
			}
			return this;
		}
	}
}
