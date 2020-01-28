using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InstagramApiSharp.Classes.Models;
using Quarkless.Models.Actions;
using Quarkless.Models.Actions.Enums.ActionTypes;
using Quarkless.Models.Actions.Factory.Action_Options;
using Quarkless.Models.Actions.Interfaces;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Common.Extensions;
using Quarkless.Models.Common.Models;
using Quarkless.Models.Common.Models.Carriers;
using Quarkless.Models.ContentInfo.Interfaces;
using Quarkless.Models.Heartbeat;
using Quarkless.Models.Heartbeat.Interfaces;
using Quarkless.Models.InstagramSearch;
using Quarkless.Models.Messaging;
using Quarkless.Models.SearchResponse;
using Quarkless.Models.Timeline;
using ActionType = Quarkless.Models.Actions.Enums.ActionType;

namespace Quarkless.Logic.Actions.Action_Instances
{
	internal class DirectMessagingAction : IActionCommit
	{
		private readonly IContentInfoBuilder _contentInfoBuilder;
		private readonly IHeartbeatLogic _heartbeatLogic;
		private readonly UserStoreDetails _user;
		private SendDirectMessageActionOptions _actionOptions;

		internal DirectMessagingAction(UserStoreDetails userStoreDetails, IContentInfoBuilder contentInfoBuilder, IHeartbeatLogic heartbeatLogic)
		{
			_contentInfoBuilder = contentInfoBuilder;
			_heartbeatLogic = heartbeatLogic;
			_user = userStoreDetails;
		}

		#region Functionality for each actions
		private async Task<long> SendDirectMessageBasedOnLikers()
		{
			var by = new By
			{
				ActionType = (int)ActionType.SendDirectMessage,
				User = _user.Profile.InstagramAccountId
			};

			var fetchMedias = (await _heartbeatLogic.GetMetaData<List<UserResponse<string>>>(new MetaDataFetchRequest
			{
				MetaDataType = MetaDataType.FetchUsersViaPostLiked,
				ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
				InstagramId = _user.ShortInstagram.Id
			}))?.Where(exclude => !exclude.SeenBy.Any(e => e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Count > 0).ToList();

			var select = fetchMedias?.ElementAtOrDefault(SecureRandom.Next(fetchMedias.Count));

			if (@select == null) return 0;
			@select.SeenBy.Add(@by);

			await _heartbeatLogic.UpdateMetaData(new MetaDataCommitRequest<List<UserResponse<string>>>
			{
				MetaDataType = MetaDataType.FetchUsersViaPostLiked,
				ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
				InstagramId = _user.ShortInstagram.Id,
				Data = select
			});

			return @select.ObjectItem?.ElementAtOrDefault(@select.ObjectItem.Count)?.UserId ?? 0;
		}
		private async Task<long> SendDirectMessageBasedOnCommenters()
		{
			var by = new By
			{
				ActionType = (int)ActionType.SendDirectMessage,
				User = _user.Profile.InstagramAccountId
			};
			var fetchMedias = (await _heartbeatLogic.GetMetaData<List<UserResponse<CommentResponse>>>(
					new MetaDataFetchRequest
					{
						MetaDataType = MetaDataType.FetchUsersViaPostCommented,
						ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
						InstagramId = _user.ShortInstagram.Id
					}))?.Where(exclude => !exclude.SeenBy.Any(e => e.User == by.User && e.ActionType == by.ActionType))
						.Where(s => s.ObjectItem.Count > 0).ToList();

			var select = fetchMedias?.ElementAtOrDefault(SecureRandom.Next(fetchMedias.Count));
			if (select == null) return 0;
			select.SeenBy.Add(by);

			await _heartbeatLogic.UpdateMetaData(new MetaDataCommitRequest<List<UserResponse<CommentResponse>>>
			{
				MetaDataType = MetaDataType.FetchUsersViaPostCommented,
				ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
				InstagramId = _user.ShortInstagram.Id,
				Data = select
			});

			return @select.ObjectItem.First().UserId;
		}
		private async Task<long> SendDirectMessageBasedOnTopic()
		{
			var by = new By
			{
				ActionType = (int)ActionType.SendDirectMessage,
				User = _user.Profile.InstagramAccountId
			};

			var fetchMedias = (await _heartbeatLogic.GetMetaData<Media>(new MetaDataFetchRequest
			{
				MetaDataType = MetaDataType.FetchMediaByTopic,
				ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
				InstagramId = _user.ShortInstagram.Id
			}))?.Where(exclude => !exclude.SeenBy.Any(e => e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Medias.Count > 0).ToList();

			var select = fetchMedias?.ElementAtOrDefault(SecureRandom.Next(fetchMedias.Count));

			if (select == null) return 0;
			select.SeenBy.Add(by);

			await _heartbeatLogic.UpdateMetaData(new MetaDataCommitRequest<Media>
			{
				MetaDataType = MetaDataType.FetchMediaByTopic,
				ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
				InstagramId = _user.ShortInstagram.Id,
				Data = select
			});

			return @select.ObjectItem?.Medias.FirstOrDefault()?.User.UserId ?? 0;
		}
		private async Task<long> SendDirectMessageBasedOnLocation()
		{
			var by = new By
			{
				ActionType = (int)ActionType.SendDirectMessage,
				User = _user.Profile.InstagramAccountId
			};
			var fetchMedias = (await _heartbeatLogic.GetMetaData<Media>(new MetaDataFetchRequest
			{
				MetaDataType = MetaDataType.FetchMediaByUserLocationTargetList,
				ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
				InstagramId = _user.ShortInstagram.Id
			}))?.Where(exclude => !exclude.SeenBy.Any(e => e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Medias.Count > 0).ToList();

			var select = fetchMedias?.ElementAtOrDefault(SecureRandom.Next(fetchMedias.Count));
			if (select == null) return 0;
			select.SeenBy.Add(by);

			await _heartbeatLogic.UpdateMetaData(new MetaDataCommitRequest<Media>
			{
				MetaDataType = MetaDataType.FetchMediaByUserLocationTargetList,
				ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
				InstagramId = _user.ShortInstagram.Id,
				Data = select
			});

			return @select.ObjectItem.Medias.FirstOrDefault()?.User.UserId ?? 0;
		}
		private async Task<long> SendDirectMessageBasedOnSuggestions()
		{
			var by = new By
			{
				ActionType = (int)ActionType.SendDirectMessage,
				User = _user.Profile.InstagramAccountId
			};
			var fetchMedias = (await _heartbeatLogic.GetMetaData<List<UserResponse<UserSuggestionDetails>>>(new MetaDataFetchRequest
			{
				MetaDataType = MetaDataType.FetchUsersFollowSuggestions,
				ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
				InstagramId = _user.ShortInstagram.Id
			}))?.Where(exclude => !exclude.SeenBy.Any(e => e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Count > 0).ToList();

			var select = fetchMedias?.ElementAtOrDefault(SecureRandom.Next(fetchMedias.Count));
			if (select == null) return 0;
			select.SeenBy.Add(by);

			await _heartbeatLogic.UpdateMetaData(new MetaDataCommitRequest<List<UserResponse<UserSuggestionDetails>>>
			{
				MetaDataType = MetaDataType.FetchUsersFollowSuggestions,
				ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
				InstagramId = _user.ShortInstagram.Id,
				Data = select
			});

			return select.ObjectItem.First().UserId;
		}
		private async Task<long> SendDirectMessageBasedOnUsersFollowers()
		{
			var by = new By
			{
				ActionType = (int)ActionType.SendDirectMessage,
				User = _user.Profile.InstagramAccountId
			};

			var fetchMedias = (await _heartbeatLogic.GetMetaData<List<UserResponse<string>>>(new MetaDataFetchRequest
			{
				MetaDataType = MetaDataType.FetchUsersFollowerList,
				ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
				InstagramId = _user.ShortInstagram.Id
			}))?.Where(exclude => !exclude.SeenBy.Any(e => e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Count > 0).ToList();

			var select = fetchMedias?.ElementAtOrDefault(SecureRandom.Next(fetchMedias.Count));
			if (select == null) return 0;
			select.SeenBy.Add(by);

			await _heartbeatLogic.UpdateMetaData(new MetaDataCommitRequest<List<UserResponse<string>>>
			{
				MetaDataType = MetaDataType.FetchUsersFollowerList,
				ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
				InstagramId = _user.ShortInstagram.Id,
				Data = select
			});

			return select.ObjectItem.First().UserId;
		}
		#endregion

		public async Task<ResultCarrier<EventActionModel>> PushAsync(DateTimeOffset executionTime)
		{
			Console.WriteLine($"Send Direct Message Action Started: {_user.OAccountId}, {_user.OInstagramAccountUsername}, {_user.OInstagramAccountUser}");
			var results = new ResultCarrier<EventActionModel>();
			try
			{
				if (!_user.Profile.AdditionalConfigurations.EnableAutoDirectMessaging)
				{
					results.IsSuccessful = false;
					results.Info = new ErrorResponse
					{
						Message =
							$"Enable Auto Direct Message Feature is turned off for {_user.OInstagramAccountUsername}"
					};
					return results;
				}
				if (_user.MessagesTemplates == null || !_user.MessagesTemplates.Any())
				{
					results.IsSuccessful = false;
					results.Info = new ErrorResponse
					{
						Message =
							$"user has no message templates, user: {_user.OAccountId}, instaId: {_user.OInstagramAccountUsername}",
						StatusCode = System.Net.HttpStatusCode.NotFound
					};
					return results;
				}

				MessagingReachType reachType;
				var messageActionsChances = new List<Chance<MessagingReachType>>();
				if (_actionOptions.MessagingReachType == MessagingReachType.Any)
				{
					var focusLocalMore = _user.Profile.AdditionalConfigurations.FocusLocalMore;
					if (_user.Profile.LocationTargetList != null && _user.Profile.LocationTargetList.Count > 0)
					{
						messageActionsChances.AddRange(new List<Chance<MessagingReachType>>
						{
							new Chance<MessagingReachType>
							{
								Object = MessagingReachType.BasedOnLocationTargetedUsers,
								Probability = focusLocalMore ? 0.35 : 0.10,
							},
							new Chance<MessagingReachType>
							{
								Object = MessagingReachType.BasedOnSuggestedUsers,
								Probability = 0.15
							},
							new Chance<MessagingReachType>
							{
								Object = MessagingReachType.BasedOnTopic,
								Probability = 0.10
							},
							new Chance<MessagingReachType>
							{
								Object = MessagingReachType.BasedOnUserFollowers,
								Probability = focusLocalMore ? 0.10 : 0.35
							},
							new Chance<MessagingReachType>
							{
								Object = MessagingReachType.BasedOnUsersFromMediaComments,
								Probability = 0.15
							},
							new Chance<MessagingReachType>
							{
								Object = MessagingReachType.BasedOnUsersFromMediaLikes,
								Probability = 0.15
							}
						});
					}
					else
					{
						messageActionsChances.AddRange(new List<Chance<MessagingReachType>>
						{
							new Chance<MessagingReachType>
							{
								Object = MessagingReachType.BasedOnSuggestedUsers,
								Probability = 0.25
							},
							new Chance<MessagingReachType>
							{
								Object = MessagingReachType.BasedOnTopic,
								Probability = 0.10
							},
							new Chance<MessagingReachType>
							{
								Object = MessagingReachType.BasedOnUserFollowers,
								Probability = 0.35
							},
							new Chance<MessagingReachType>
							{
								Object = MessagingReachType.BasedOnUsersFromMediaComments,
								Probability = 0.15
							},
							new Chance<MessagingReachType>
							{
								Object = MessagingReachType.BasedOnUsersFromMediaLikes,
								Probability = 0.15
							}
						});
					}

					reachType = SecureRandom.ProbabilityRoll(messageActionsChances);
				}
				else
				{
					reachType = _actionOptions.MessagingReachType;
				}

				var nominatedIds = new List<string>();

				switch (reachType)
				{
					case MessagingReachType.BasedOnLocationTargetedUsers:
						for (var i = 0; i < _actionOptions.Limit; i++)
						{
							var locNom = (await SendDirectMessageBasedOnLocation()).ToString();
							nominatedIds.Add(locNom);
						}
						break;
					case MessagingReachType.BasedOnTopic:
						for (var i = 0; i < _actionOptions.Limit; i++)
						{
							var topicNom = (await SendDirectMessageBasedOnTopic()).ToString();
							nominatedIds.Add(topicNom);
						}
						break;
					case MessagingReachType.BasedOnUserFollowers:
						for (var i = 0; i < _actionOptions.Limit; i++)
						{
							var userNom = (await SendDirectMessageBasedOnUsersFollowers()).ToString();
							nominatedIds.Add(userNom);
						}
						break;
					case MessagingReachType.BasedOnSuggestedUsers:
						for (var i = 0; i < _actionOptions.Limit; i++)
						{
							var suggestNom = (await SendDirectMessageBasedOnSuggestions()).ToString();
							nominatedIds.Add(suggestNom);
						}
						break;
					case MessagingReachType.BasedOnUsersFromMediaComments:
						for (var i = 0; i < _actionOptions.Limit; i++)
						{
							var commentNom = (await SendDirectMessageBasedOnCommenters()).ToString();
							nominatedIds.Add(commentNom);
						}
						break;
					case MessagingReachType.BasedOnUsersFromMediaLikes:
						for (var i = 0; i < _actionOptions.Limit; i++)
						{
							var likeNom = (await SendDirectMessageBasedOnLikers()).ToString();
							nominatedIds.Add(likeNom);
						}
						break;
				}

				if (nominatedIds.Count <= 0 || nominatedIds.All(x => (long.Parse(x) == 0)))
				{
					results.IsSuccessful = false;
					results.Info = new ErrorResponse
					{
						Message = $"could not find a nominated person to send message to, user: {_user.OAccountId}, instaId: {_user.OInstagramAccountUsername}",
						StatusCode = System.Net.HttpStatusCode.NotFound
					};
					return results;
				}

				if (nominatedIds.Any(x => x == null || x == "0"))
				{
					nominatedIds = nominatedIds.Where(x => x != null && x != "0").ToList();
				}
				var templateSelected = _user.MessagesTemplates.TakeAny(1).First();

				var @event = new EventActionModel($"sendDirectMessage_{reachType.ToString()}")
				{
					ActionType = ActionType.SendDirectMessage,
					User = new UserStore
					{
						OAccountId = _user.OAccountId,
						OInstagramAccountUsername = _user.OInstagramAccountUsername,
						OInstagramAccountUser = _user.OInstagramAccountUser
					}
				};

				switch ((MessageActionType)templateSelected.Type)
				{
					case MessageActionType.Text:
					{
						var textModel = new SendDirectTextModel
						{
							Recipients = nominatedIds,
							TextMessage = templateSelected?.Entity.Message
						};
						@event.DataObjects.Add(new EventBody(textModel, textModel.GetType(), executionTime));
						break;
					}
					case MessageActionType.Link:
					{
						var linkModel = new SendDirectLinkModel
						{
							Recipients = nominatedIds,
							Link = templateSelected?.Entity.Link,
							TextMessage = templateSelected?.Entity.Message
						};
						@event.DataObjects.Add(new EventBody(linkModel, linkModel.GetType(), executionTime));
						break;
					}
					case MessageActionType.Photo:
					{
						var photoModel = new SendDirectPhotoModel
						{
							Recipients = nominatedIds,
							Image = new InstaImage
							{
								ImageBytes = Convert.FromBase64String(templateSelected?.Entity.MediaBytes.Split(',')[1])
							}
						};
						@event.DataObjects.Add(new EventBody(photoModel, photoModel.GetType(), executionTime));
						break;
					}
					case MessageActionType.Video:
					{
						var videoModel = new SendDirectVideoModel
						{
							Recipients = nominatedIds,
							Video = new InstaVideoUpload
							{
								Video = new InstaVideo
								{
									VideoBytes =
										Convert.FromBase64String(templateSelected?.Entity.MediaBytes.Split(',')[1])
								},
								VideoThumbnail = new InstaImage
								{
									ImageBytes = _actionOptions.PostAnalyser.Manipulation.VideoEditor
										.GenerateVideoThumbnail(
											Convert.FromBase64String(templateSelected?.Entity.MediaBytes.Split(',')[1]))
								}
							}
						};
						@event.DataObjects.Add(new EventBody(videoModel, videoModel.GetType(), executionTime));
						break;
					}
					case MessageActionType.Profile:
					{
						throw new NotImplementedException();
					}
					default:throw new Exception("Invalid Message Action type");
				}

				if (!results.Results.DataObjects.Any())
				{
					results.IsSuccessful = false;
					results.Info = new ErrorResponse
					{
						Message = "Failed at serializing send message type"
					};
					return results;
				}

				results.IsSuccessful = true;
				results.Results = @event;
				return results;
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
				Console.WriteLine($"Send Direct Message Action Ended: { _user.OAccountId}, { _user.OInstagramAccountUsername}, { _user.OInstagramAccountUser}");
			}
		}

		public IActionCommit ModifyOptions(IActionOptions newOptions)
		{
			try
			{
				_actionOptions = newOptions as SendDirectMessageActionOptions;
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
			}
			return this;
		}
	}
}
