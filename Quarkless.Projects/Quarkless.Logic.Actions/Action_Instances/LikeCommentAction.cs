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
using Quarkless.Models.Common.Models.Resolver;
using Quarkless.Models.ContentInfo.Interfaces;
using Quarkless.Models.Heartbeat;
using Quarkless.Models.Lookup.Interfaces;
using Quarkless.Models.SearchResponse;

namespace Quarkless.Logic.Actions.Action_Instances
{
	internal class LikeCommentAction : BaseAction, IActionCommit
	{
		private readonly IContentInfoBuilder _contentInfoBuilder;
		private readonly IHeartbeatLogic _heartbeatLogic;
		private readonly UserStoreDetails _user;
		private LikeCommentActionOptions _actionOptions;
		internal LikeCommentAction(UserStoreDetails userStoreDetails, IContentInfoBuilder contentInfoBuilder,
			IHeartbeatLogic heartbeatLogic, ILookupLogic lookupLogic)
			: base(lookupLogic, ActionType.LikeComment, userStoreDetails)
		{
			_user = userStoreDetails;
			_contentInfoBuilder = contentInfoBuilder;
			_heartbeatLogic = heartbeatLogic;
			_actionOptions = new LikeCommentActionOptions();
		}

		private async Task<LikeCommentRequest> CommentingByTopic()
		{
			const MetaDataType fetchType = MetaDataType.FetchUsersViaPostCommented;
			var lookups = await GetLookupItems();
			var fetchComments = (await _heartbeatLogic.GetMetaData<List<UserResponse<InstaComment>>>(new MetaDataFetchRequest
			{
				MetaDataType = fetchType,
				ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
				InstagramId = _user.ShortInstagram.Id,
				AccountId = _user.AccountId
			}))?.Where(s => s.ObjectItem.Count > 0)
				.SelectMany(_=>_.ObjectItem)
				.Where(_=> !lookups.Exists(l => l.ObjectId == _.UserId.ToString()))
				.ToList();

			var comment = fetchComments?.ElementAtOrDefault(SecureRandom.Next(fetchComments.Count - 1));

			if (comment?.Object?.Pk == null) return null;

			await AddObjectToLookup(comment.UserId.ToString());

			return new LikeCommentRequest
			{
				CommentId = comment.Object.Pk,
				CommentLiked = comment.Object.Text,
				User = new UserShort
				{
					Id = comment.UserId,
					ProfilePicture = comment.ProfilePicture,
					Username = comment.Username
				},
				DataFrom = new DataFrom
				{
					TopicName = comment.Topic?.Name,
					NominatedFrom = fetchType
				}
			};
		}
		private async Task<LikeCommentRequest> CommentingByCommenters()
		{
			const MetaDataType fetchType = MetaDataType.FetchCommentsViaPostCommented;
			var lookups = await GetLookupItems();
			var fetchComments = (await _heartbeatLogic.GetMetaData<List<UserResponse<InstaComment>>>(
				new MetaDataFetchRequest
			{
				MetaDataType = fetchType,
				ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
				InstagramId = _user.ShortInstagram.Id,
				AccountId = _user.AccountId
			}))?.Where(s => s.ObjectItem.Count > 0)
				.SelectMany(_ => _.ObjectItem)
				.Where(_ => !lookups.Exists(l => l.ObjectId == _.UserId.ToString()))
				.ToList();

			var comment = fetchComments?.ElementAtOrDefault(SecureRandom.Next(fetchComments.Count - 1));

			if (comment?.Object?.Pk == null) return null;
			await AddObjectToLookup(comment.UserId.ToString());
			return new LikeCommentRequest
			{
				CommentId = comment.Object.Pk,
				CommentLiked = comment.Object.Text,
				User = new UserShort
				{
					Id = comment.UserId,
					ProfilePicture = comment.ProfilePicture,
					Username = comment.Username
				},
				DataFrom = new DataFrom
				{
					TopicName = comment.Topic?.Name,
					NominatedFrom = fetchType
				}
			};
		}
		private async Task<LikeCommentRequest> CommentingByLikers()
		{
			const MetaDataType fetchType = MetaDataType.FetchCommentsViaPostsLiked;
			var lookups = await GetLookupItems();
			var fetchComments = (await _heartbeatLogic.GetMetaData<List<UserResponse<InstaComment>>>(
			new MetaDataFetchRequest
				{
					MetaDataType = MetaDataType.FetchCommentsViaPostsLiked,
					ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
					InstagramId = _user.ShortInstagram.Id,
					AccountId = _user.AccountId
				}))?.Where(s => s.ObjectItem.Count > 0)
				.SelectMany(_ => _.ObjectItem)
				.Where(_ => !lookups.Exists(l => l.ObjectId == _.UserId.ToString()))
				.ToList();

			var comment = fetchComments?.ElementAtOrDefault(SecureRandom.Next(fetchComments.Count - 1));

			if (comment?.Object?.Pk == null) return null;
			await AddObjectToLookup(comment.UserId.ToString());
			return new LikeCommentRequest
			{
				CommentId = comment.Object.Pk,
				CommentLiked = comment.Object.Text,
				User = new UserShort
				{
					Id = comment.UserId,
					ProfilePicture = comment.ProfilePicture,
					Username = comment.Username
				},
				DataFrom = new DataFrom
				{
					TopicName = comment.Topic?.Name,
					NominatedFrom = fetchType
				}
			};
		}
		private async Task<LikeCommentRequest> CommentingByTarget()
		{
			const MetaDataType fetchType = MetaDataType.FetchCommentsViaUserTargetList;
			var lookups = await GetLookupItems();
			var fetchComments = (await _heartbeatLogic.GetMetaData<List<UserResponse<InstaComment>>>(
				new MetaDataFetchRequest
			{
				MetaDataType = fetchType,
				ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
				InstagramId = _user.ShortInstagram.Id,
				AccountId = _user.AccountId
			}))?.Where(s => s.ObjectItem.Count > 0)
				.SelectMany(_ => _.ObjectItem)
				.Where(_ => !lookups.Exists(l => l.ObjectId == _.UserId.ToString()))
				.ToList();

			var comment = fetchComments?.ElementAtOrDefault(SecureRandom.Next(fetchComments.Count - 1));

			if (comment?.Object?.Pk == null) return null;
			await AddObjectToLookup(comment.UserId.ToString());
			return new LikeCommentRequest
			{
				CommentId = comment.Object.Pk,
				CommentLiked = comment.Object.Text,
				User = new UserShort
				{
					Id = comment.UserId,
					ProfilePicture = comment.ProfilePicture,
					Username = comment.Username
				},
				DataFrom = new DataFrom
				{
					TopicName = comment.Topic?.Name,
					NominatedFrom = fetchType
				}
			};
		}
		private async Task<LikeCommentRequest> CommentingByLocation()
		{
			const MetaDataType fetchType = MetaDataType.FetchCommentsViaLocationTargetList;
			var lookups = await GetLookupItems();
			var fetchComments = (await _heartbeatLogic.GetMetaData<List<UserResponse<InstaComment>>>(
				new MetaDataFetchRequest
			{
				MetaDataType = fetchType,
				ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
				InstagramId = _user.ShortInstagram.Id,
				AccountId = _user.AccountId
			}))?.Where(s => s.ObjectItem.Count > 0)
				.SelectMany(_ => _.ObjectItem)
				.Where(_ => !lookups.Exists(l => l.ObjectId == _.UserId.ToString()))
				.ToList();

			var comment = fetchComments?.ElementAtOrDefault(SecureRandom.Next(fetchComments.Count - 1));

			if (comment?.Object?.Pk == null) return null;
			await AddObjectToLookup(comment.UserId.ToString());
			return new LikeCommentRequest
			{
				CommentId = comment.Object.Pk,
				CommentLiked = comment.Object.Text,
				User = new UserShort
				{
					Id = comment.UserId,
					ProfilePicture = comment.ProfilePicture,
					Username = comment.Username
				},
				DataFrom = new DataFrom
				{
					TopicName = comment.Topic?.Name,
					NominatedFrom = fetchType
				}
			};
		}
		private async Task<LikeCommentRequest> CommentingByUserFeed()
		{
			const MetaDataType fetchType = MetaDataType.FetchCommentsViaUserFeed;
			var lookups = await GetLookupItems();
			var fetchComments = (await _heartbeatLogic.GetMetaData<List<UserResponse<InstaComment>>>(
			new MetaDataFetchRequest
			{
				MetaDataType = fetchType,
				ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
				InstagramId = _user.ShortInstagram.Id,
				AccountId = _user.AccountId
			}))?.Where(s => s.ObjectItem.Count > 0)
				.SelectMany(_ => _.ObjectItem)
				.Where(_ => !lookups.Exists(l => l.ObjectId == _.UserId.ToString()))
				.ToList();

			var comment = fetchComments?.ElementAtOrDefault(SecureRandom.Next(fetchComments.Count - 1));

			if (comment?.Object?.Pk == null) return null;
			await AddObjectToLookup(comment.UserId.ToString());

			return new LikeCommentRequest
			{
				CommentId = comment.Object.Pk,
				CommentLiked = comment.Object.Text,
				User = new UserShort
				{
					Id = comment.UserId,
					ProfilePicture = comment.ProfilePicture,
					Username = comment.Username
				},
				DataFrom = new DataFrom
				{
					TopicName = comment.Topic?.Name,
					NominatedFrom = fetchType
				}
			};
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
						LikeCommentRequest nominatedComment = null;
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
						@event.DataObjects.Add(new EventBody(nominatedComment, nominatedComment.GetType(), executionTime));
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
