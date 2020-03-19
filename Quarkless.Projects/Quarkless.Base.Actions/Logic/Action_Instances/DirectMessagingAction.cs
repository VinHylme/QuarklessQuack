using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InstagramApiSharp.Classes.Models;
using Quarkless.Base.Actions.Models;
using Quarkless.Base.Actions.Models.Enums.ActionTypes;
using Quarkless.Base.Actions.Models.Factory.Action_Options;
using Quarkless.Base.Actions.Models.Interfaces;
using Quarkless.Base.ContentInfo.Models.Interfaces;
using Quarkless.Base.Heartbeat.Models;
using Quarkless.Base.Heartbeat.Models.Interfaces;
using Quarkless.Base.InstagramSearch.Models;
using Quarkless.Base.Lookup.Models.Interfaces;
using Quarkless.Base.Messaging.Models;
using Quarkless.Common.Timeline.Models;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Common.Extensions;
using Quarkless.Models.Common.Models;
using Quarkless.Models.Common.Models.Carriers;
using Quarkless.Models.SearchResponse;

namespace Quarkless.Base.Actions.Logic.Action_Instances
{
	internal class DirectMessagingAction : BaseAction, IActionCommit
	{
		private readonly IContentInfoBuilder _contentInfoBuilder;
		private readonly IHeartbeatLogic _heartbeatLogic;
		private readonly UserStoreDetails _user;
		private SendDirectMessageActionOptions _actionOptions;

		internal DirectMessagingAction(UserStoreDetails userStoreDetails,
			IContentInfoBuilder contentInfoBuilder, IHeartbeatLogic heartbeatLogic,
			ILookupLogic lookupLogic) : base(lookupLogic, ActionType.SendDirectMessage, userStoreDetails)
		{
			_contentInfoBuilder = contentInfoBuilder;
			_heartbeatLogic = heartbeatLogic;
			_user = userStoreDetails;
		}

		#region Functionality for each actions
		private async Task<long> SendDirectMessageBasedOnLikers()
		{
			var lookups = await GetLookupItems();
			var fetchMedias = (await _heartbeatLogic.GetMetaData<List<UserResponse<string>>>(new MetaDataFetchRequest
			{
				MetaDataType = MetaDataType.FetchUsersViaPostLiked,
				ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
				InstagramId = _user.ShortInstagram.Id,
				AccountId = _user.AccountId
			}))
				?.Where(s => s.ObjectItem.Count > 0)
				.SelectMany(_=>_.ObjectItem)
				.Where(_=> !lookups.Exists(l => l.ObjectId == _.UserId.ToString()))
				.ToList();

			var objectItem = fetchMedias?.ElementAtOrDefault(SecureRandom.Next(fetchMedias.Count - 1));

			if (objectItem?.UserId == null) return 0;

			await AddObjectToLookup(objectItem.UserId.ToString());

			return objectItem.UserId;
		}
		private async Task<long> SendDirectMessageBasedOnCommenters()
		{
			var lookups = await GetLookupItems();
			var fetchMedias = (await _heartbeatLogic.GetMetaData<List<UserResponse<CommentResponse>>>(
			new MetaDataFetchRequest
			{
				MetaDataType = MetaDataType.FetchUsersViaPostCommented,
				ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
				InstagramId = _user.ShortInstagram.Id,
				AccountId = _user.AccountId
			}))?.Where(s => s.ObjectItem.Count > 0)
				.SelectMany(_ => _.ObjectItem)
				.Where(_ => !lookups.Exists(l => l.ObjectId == _.UserId.ToString()))
				.ToList();

			var objectItem = fetchMedias?.ElementAtOrDefault(SecureRandom.Next(fetchMedias.Count - 1));

			if (objectItem?.UserId == null) return 0;

			await AddObjectToLookup(objectItem.UserId.ToString());

			return objectItem.UserId;
		}
		private async Task<long> SendDirectMessageBasedOnTopic()
		{
			var lookups = await GetLookupItems();
			var fetchMedias = (await _heartbeatLogic.GetMetaData<Quarkless.Models.SearchResponse.Media>(new MetaDataFetchRequest
			{
				MetaDataType = MetaDataType.FetchMediaByTopic,
				ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
				InstagramId = _user.ShortInstagram.Id,
				AccountId = _user.AccountId
			}))
				?.Where(s => s.ObjectItem.Medias.Count > 0)
				.SelectMany(_ => _.ObjectItem.Medias)
				.Where(_ => !lookups.Exists(l => l.ObjectId == _.User.UserId.ToString()))
				.ToList();

			var objectItem = fetchMedias?.ElementAtOrDefault(SecureRandom.Next(fetchMedias.Count - 1));

			if (objectItem?.MediaId == null) return 0;

			await AddObjectToLookup(objectItem.User.UserId.ToString());

			return objectItem.User.UserId;
		}
		private async Task<long> SendDirectMessageBasedOnLocation()
		{
			var lookups = await GetLookupItems();
			var fetchMedias = (await _heartbeatLogic.GetMetaData<Quarkless.Models.SearchResponse.Media>(new MetaDataFetchRequest
			{
				MetaDataType = MetaDataType.FetchMediaByUserLocationTargetList,
				ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
				InstagramId = _user.ShortInstagram.Id,
				AccountId = _user.AccountId
			}))?.Where(s => s.ObjectItem.Medias.Count > 0)
				.SelectMany(_ => _.ObjectItem.Medias)
				.Where(_ => !lookups.Exists(l => l.ObjectId == _.User.UserId.ToString()))
				.ToList();

			var objectItem = fetchMedias?.ElementAtOrDefault(SecureRandom.Next(fetchMedias.Count - 1));
			if (objectItem?.MediaId == null) return 0;

			await AddObjectToLookup(objectItem.User.UserId.ToString());

			return objectItem.User.UserId;
		}
		private async Task<long> SendDirectMessageBasedOnSuggestions()
		{
			var lookups = await GetLookupItems();
			var fetchMedias = (await _heartbeatLogic.GetMetaData<List<UserResponse<UserSuggestionDetails>>>(new MetaDataFetchRequest
			{
				MetaDataType = MetaDataType.FetchUsersFollowSuggestions,
				ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
				InstagramId = _user.ShortInstagram.Id,
				AccountId = _user.AccountId
			}))?.Where(s => s.ObjectItem.Count > 0)
				.SelectMany(_ => _.ObjectItem)
				.Where(_ => !lookups.Exists(l => l.ObjectId == _.UserId.ToString()))
				.ToList();

			var objectItem = fetchMedias?.ElementAtOrDefault(SecureRandom.Next(fetchMedias.Count - 1));
			if (objectItem?.UserId == null) return 0;

			await AddObjectToLookup(objectItem.UserId.ToString());

			return objectItem.UserId;
		}
		private async Task<long> SendDirectMessageBasedOnUsersFollowers()
		{
			var lookups = await GetLookupItems();
			var fetchMedias = (await _heartbeatLogic.GetMetaData<List<UserResponse<string>>>(new MetaDataFetchRequest
			{
				MetaDataType = MetaDataType.FetchUsersFollowerList,
				ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
				InstagramId = _user.ShortInstagram.Id,
				AccountId = _user.AccountId
			}))?.Where(s => s.ObjectItem.Count > 0)
				.SelectMany(_ => _.ObjectItem)
				.Where(_ => !lookups.Exists(l => l.ObjectId == _.UserId.ToString()))
				.ToList();

			var objectItem = fetchMedias?.ElementAtOrDefault(SecureRandom.Next(fetchMedias.Count - 1));

			if (objectItem?.UserId == null) return 0;

			await AddObjectToLookup(objectItem.UserId.ToString());

			return objectItem.UserId;
		}
		#endregion

		public async Task<ResultCarrier<EventActionModel>> PushAsync(DateTimeOffset executionTime)
		{
			Console.WriteLine($"Send Direct Message Action Started: {_user.AccountId}, {_user.InstagramAccountUsername}, {_user.InstagramAccountUser}");
			var results = new ResultCarrier<EventActionModel>();
			try
			{
				if (!_user.Profile.AdditionalConfigurations.EnableAutoDirectMessaging)
				{
					results.IsSuccessful = false;
					results.Info = new ErrorResponse
					{
						Message =
							$"Enable Auto Direct Message Feature is turned off for {_user.InstagramAccountUsername}"
					};
					return results;
				}
				if (_user.MessagesTemplates == null || !_user.MessagesTemplates.Any())
				{
					results.IsSuccessful = false;
					results.Info = new ErrorResponse
					{
						Message =
							$"user has no message templates, user: {_user.AccountId}, instaId: {_user.InstagramAccountUsername}",
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
						Message = $"could not find a nominated person to send message to, user: {_user.AccountId}, instaId: {_user.InstagramAccountUsername}",
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
						AccountId = _user.AccountId,
						InstagramAccountUsername = _user.InstagramAccountUsername,
						InstagramAccountUser = _user.InstagramAccountUser
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
				Console.WriteLine($"Send Direct Message Action Ended: { _user.AccountId}, { _user.InstagramAccountUsername}, { _user.InstagramAccountUser}");
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
