using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using InstagramApiSharp.Classes.Models;
using Quarkless.MediaAnalyser;
using Quarkless.Services.Interfaces;
using Quarkless.Services.Interfaces.Actions;
using Quarkless.Services.StrategyBuilders;
using QuarklessContexts.Classes.Carriers;
using QuarklessContexts.Enums;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models;
using QuarklessContexts.Models.MessagingModels;
using QuarklessContexts.Models.ServicesModels.HeartbeatModels;
using QuarklessContexts.Models.ServicesModels.SearchModels;
using QuarklessContexts.Models.Timeline;
using QuarklessLogic.Handlers.RequestBuilder.Consts;
using QuarklessLogic.Logic.StorageLogic;
using QuarklessLogic.ServicesLogic.HeartbeatLogic;

namespace Quarkless.Services.ActionBuilders.EngageActions
{
	public enum MessageActionType
	{
		Text,
		Link,
		Photo,
		Video,
		Voice,
		Gif,
		Profile,
		Share,
		Location,
		Hashtag
	}
	public enum MessagingReachType
	{
		Any,
		BasedOnSuggestedUsers,
		BasedOnLocationTargetedUsers,
		BasedOnUserFollowers,
		BasedOnOtherUserFollower,
		BasedOnTopic,
		BasedOnUsersFromMediaLikes,
		BasedOnUsersFromMediaComments
	}
	public class DirectMessagingAction : IActionCommit
	{
		private readonly IContentManager _builder;
		private readonly IHeartbeatLogic _heartbeatLogic;
		private UserStoreDetails user;
		public DirectMessagingAction(IContentManager contentManager, IHeartbeatLogic heartbeatLogic)
		{
			_heartbeatLogic = heartbeatLogic;
			_builder = contentManager;
		}

		#region Functionality for each actions
		private long SendDirectMessageBasedOnLikers()
		{
			var by = new By
			{
				ActionType = (int)ActionType.SendDirectMessage,
				User = user.Profile.InstagramAccountId
			};
			var fetchMedias = _heartbeatLogic.GetMetaData<List<UserResponse<string>>>(MetaDataType.FetchUsersViaPostLiked, user.Profile.Topics.TopicFriendlyName)
				.GetAwaiter().GetResult().Where(exclude=>!exclude.SeenBy.Any(e=>e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Count > 0);
			var meta_S = fetchMedias as __Meta__<List<UserResponse<string>>>[] ?? fetchMedias.ToArray();
			var select = meta_S.ElementAtOrDefault(SecureRandom.Next(meta_S.Count()));
			if (@select == null) return 0;
			@select.SeenBy.Add(@by);
			_heartbeatLogic.UpdateMetaData(MetaDataType.FetchUsersViaPostLiked, user.Profile.Topics.TopicFriendlyName, @select).GetAwaiter().GetResult();
			return @select.ObjectItem?.ElementAtOrDefault(@select.ObjectItem.Count)?.UserId ?? 0;
		}
		private long SendDirectMessageBasedOnCommenters()
		{
			var by = new By
			{
				ActionType = (int)ActionType.SendDirectMessage,
				User = user.Profile.InstagramAccountId
			};
			var fetchMedias = _heartbeatLogic.GetMetaData<List<UserResponse<CommentResponse>>>(MetaDataType.FetchUsersViaPostCommented, user.Profile.Topics.TopicFriendlyName)
				.GetAwaiter().GetResult().Where(exclude=>!exclude.SeenBy.Any(e=>e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Count > 0);
			var meta_S = fetchMedias as __Meta__<List<UserResponse<CommentResponse>>>[] ?? fetchMedias.ToArray();
			var select = meta_S.ElementAtOrDefault(SecureRandom.Next(meta_S.Length));
			if (@select == null) return 0;
			@select.SeenBy.Add(@by);
			_heartbeatLogic.UpdateMetaData(MetaDataType.FetchUsersViaPostCommented, user.Profile.Topics.TopicFriendlyName, @select).GetAwaiter().GetResult();
			return @select.ObjectItem.First().UserId;
		}
		private long SendDirectMessageBasedOnTopic()
		{
			var by = new By
			{
				ActionType = (int)ActionType.SendDirectMessage,
				User = user.Profile.InstagramAccountId
			};
			var fetchMedias = _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaByTopic, user.Profile.Topics.TopicFriendlyName)
				.GetAwaiter().GetResult().Where(exclude=>!exclude.SeenBy.Any(e=>e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Medias.Count > 0);
			if (fetchMedias == null) return 0;
			var meta_S = fetchMedias as __Meta__<Media>[] ?? fetchMedias.ToArray();
			var select = meta_S.ElementAtOrDefault(SecureRandom.Next(meta_S.Count()));
			if (@select == null) return 0;
			@select.SeenBy.Add(@by);
			_heartbeatLogic.UpdateMetaData(MetaDataType.FetchMediaByTopic, user.Profile.Topics.TopicFriendlyName, @select).GetAwaiter().GetResult();
			return @select.ObjectItem?.Medias.FirstOrDefault()?.User.UserId ?? 0;
		}
		private long SendDirectMessageBasedOnLocation()
		{
			var by = new By
			{
				ActionType = (int)ActionType.SendDirectMessage,
				User = user.Profile.InstagramAccountId
			};
			var fetchMedias = _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaByUserLocationTargetList, user.Profile.Topics.TopicFriendlyName, user.Profile.InstagramAccountId)
				.GetAwaiter().GetResult().Where(exclude=>!exclude.SeenBy.Any(e=>e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Medias.Count > 0);
			var meta_S = fetchMedias as __Meta__<Media>[] ?? fetchMedias.ToArray();
			var select = meta_S.ElementAtOrDefault(SecureRandom.Next(meta_S.Count()));
			if (@select == null) return 0;
			@select.SeenBy.Add(@by);
			_heartbeatLogic.UpdateMetaData(MetaDataType.FetchMediaByUserLocationTargetList, user.Profile.Topics.TopicFriendlyName, @select,user.Profile.InstagramAccountId).GetAwaiter().GetResult();
			return @select.ObjectItem.Medias.FirstOrDefault()?.User.UserId ?? 0;
		}
		private long SendDirectMessageBasedOnSuggestions()
		{
			var by = new By
			{
				ActionType = (int)ActionType.SendDirectMessage,
				User = user.Profile.InstagramAccountId
			};
			var fetchMedias = _heartbeatLogic.GetMetaData<List<UserResponse<UserSuggestionDetails>>>(MetaDataType.FetchUsersFollowSuggestions, user.Profile.Topics.TopicFriendlyName, user.OInstagramAccountUser)
				.GetAwaiter().GetResult().Where(exclude=>!exclude.SeenBy.Any(e=>e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Count > 0);

			var meta_S = fetchMedias as __Meta__<List<UserResponse<UserSuggestionDetails>>>[] ?? fetchMedias.ToArray();
			var select = meta_S.ElementAtOrDefault(SecureRandom.Next(meta_S.Count()));
			if (@select == null) return 0;
			@select.SeenBy.Add(@by);

			_heartbeatLogic.UpdateMetaData(MetaDataType.FetchUsersFollowSuggestions, user.Profile.Topics.TopicFriendlyName, @select).GetAwaiter().GetResult();
			return @select.ObjectItem.First().UserId;
		}
		private long SendDirectMessageBasedOnUsersFollowers()
		{
			var by = new By
			{
				ActionType = (int)ActionType.SendDirectMessage,
				User = user.Profile.InstagramAccountId
			};
			var fetchMedias = _heartbeatLogic.GetMetaData<List<UserResponse<string>>>(MetaDataType.FetchUsersFollowerList, user.Profile.Topics.TopicFriendlyName, user.OInstagramAccountUser)
				.GetAwaiter().GetResult().Where(exclude=>!exclude.SeenBy.Any(e=>e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Count > 0);

			var meta_S = fetchMedias as __Meta__<List<UserResponse<string>>>[] ?? fetchMedias.ToArray();
			var select = meta_S.ElementAtOrDefault(SecureRandom.Next(meta_S.Count()));
			if (@select == null) return 0;
			@select.SeenBy.Add(@by);

			_heartbeatLogic.UpdateMetaData(MetaDataType.FetchUsersFollowerList, user.Profile.Topics.TopicFriendlyName, @select).GetAwaiter().GetResult();
			return @select.ObjectItem.First().UserId;
		}
		#endregion

		public ResultCarrier<IEnumerable<TimelineEventModel>> Push(IActionOptions actionOptions)
		{
			Console.WriteLine($"Send Direct Message Action Started: {user.OAccountId}, {user.OInstagramAccountUsername}, {user.OInstagramAccountUser}");
			var results = new ResultCarrier<IEnumerable<TimelineEventModel>>();
			try
			{
				if (!(actionOptions is SendDirectMessageActionOptions directMessageOptions)) return results;
				if (user == null)
				{
					results.IsSuccesful = false;
					results.Info = new ErrorResponse
					{
						Message = $"user is null, user: {user.OAccountId}, instaId: {user.OInstagramAccountUsername}",
						StatusCode = System.Net.HttpStatusCode.NotFound
					};
					return results;
				}

				if (!user.Profile.AdditionalConfigurations.EnableAutoDirectMessaging) return results;
				if (user.MessagesTemplates == null || !user.MessagesTemplates.Any())
				{
					results.IsSuccesful = false;
					results.Info = new ErrorResponse
					{
						Message =
							$"user has no message templates, user: {user.OAccountId}, instaId: {user.OInstagramAccountUsername}",
						StatusCode = System.Net.HttpStatusCode.NotFound
					};
					return results;
				}

				var reachType = MessagingReachType.BasedOnTopic;
				var messageActionsChances = new List<Chance<MessagingReachType>>();
				if (directMessageOptions.MessagingReachType == MessagingReachType.Any)
				{
					var focusLocalMore = user.Profile.AdditionalConfigurations.FocusLocalMore;
					if (user.Profile.LocationTargetList != null && user.Profile.LocationTargetList.Count > 0)
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
					reachType = directMessageOptions.MessagingReachType;
				}

				var nominatedIds = new List<string>();
				switch (reachType)
				{
					case MessagingReachType.BasedOnLocationTargetedUsers:
						for (var i = 0; i < directMessageOptions.Limit; i++)
						{
							var locNom = SendDirectMessageBasedOnLocation().ToString();
							nominatedIds.Add(locNom);
						}
						break;
					case MessagingReachType.BasedOnTopic:
						for (var i = 0; i < directMessageOptions.Limit; i++)
						{
							var topicNom = SendDirectMessageBasedOnTopic().ToString();
							nominatedIds.Add(topicNom);
						}
						break;
					case MessagingReachType.BasedOnUserFollowers:
						for (var i = 0; i < directMessageOptions.Limit; i++)
						{
							var userNom = SendDirectMessageBasedOnUsersFollowers().ToString();
							nominatedIds.Add(userNom);
						}
						break;
					case MessagingReachType.BasedOnSuggestedUsers:
						for (var i = 0; i < directMessageOptions.Limit; i++)
						{
							var suggestNom = SendDirectMessageBasedOnSuggestions().ToString();
							nominatedIds.Add(suggestNom);
						}
						break;
					case MessagingReachType.BasedOnUsersFromMediaComments:
						for (var i = 0; i < directMessageOptions.Limit; i++)
						{
							var commentNom = SendDirectMessageBasedOnCommenters().ToString();
							nominatedIds.Add(commentNom);
						}
						break;
					case MessagingReachType.BasedOnUsersFromMediaLikes:
						for (var i = 0; i < directMessageOptions.Limit; i++)
						{
							var likeNom = SendDirectMessageBasedOnLikers().ToString();
							nominatedIds.Add(likeNom);
						}
						break;
				}
				if (nominatedIds.Count <= 0 || nominatedIds.All(x=>(long.Parse(x) == 0)))
				{
					results.IsSuccesful = false;
					results.Info = new ErrorResponse
					{
						Message =
							$"could not find a nominated person to send message to, user: {user.OAccountId}, instaId: {user.OInstagramAccountUsername}",
						StatusCode = System.Net.HttpStatusCode.NotFound
					};
					return results;
				}

				if (nominatedIds.Any(x => x == null || x == "0"))
				{
					nominatedIds = nominatedIds.Where(x => x != null && x != "0").ToList();
				}
				var templateSelected = user.MessagesTemplates.TakeAny(1).First();

				var restModel = new RestModel
				{
					RequestType = RequestType.POST,
					User = user
				};

				switch ((MessageActionType) templateSelected.Type)
				{
					case MessageActionType.Text:
						var textModel = new SendDirectTextModel
						{
							Recipients = nominatedIds,
							TextMessage = templateSelected?.Entity.Message
						};
						restModel.BaseUrl = UrlConstants.SendDirectMessageText;
						restModel.JsonBody = textModel.ToJsonString();
						break;
					case MessageActionType.Link:
						var linkModel = new SendDirectLinkModel
						{
							Recipients = nominatedIds,
							Link = templateSelected?.Entity.Link,
							TextMessage = templateSelected?.Entity.Message
						};
						restModel.BaseUrl = UrlConstants.SendDirectMessageLink;
						restModel.JsonBody = linkModel.ToJsonString();
						break;
					case MessageActionType.Photo:
						var photoModel = new SendDirectPhotoModel
						{
							Recipients = nominatedIds,
							Image = new InstaImage
							{
								ImageBytes = Convert.FromBase64String(templateSelected?.Entity.MediaBytes.Split(',')[1])
							}
						};
						restModel.BaseUrl = UrlConstants.SendDirectMessagePhoto;
						restModel.JsonBody = photoModel.ToJsonString();
						break;
					case MessageActionType.Video:
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
									ImageBytes = Convert
										.FromBase64String(templateSelected?.Entity.MediaBytes.Split(',')[1])
										.GenerateVideoThumbnail().GetAwaiter().GetResult()
								}
							}
						};
						restModel.BaseUrl = UrlConstants.SendDirectMessageVideo;
						restModel.JsonBody = videoModel.ToJsonString();
						break;
					case MessageActionType.Profile:
						break;
				}

				if (string.IsNullOrEmpty(restModel.BaseUrl) || string.IsNullOrEmpty(restModel.JsonBody))
				{
					results.IsSuccesful = false;
					results.Info = new ErrorResponse
					{
						Message = "Failed at serializing send message type"
					};
					return results;
				}

				results.IsSuccesful = true;
				results.Results = new List<TimelineEventModel>
				{
					new TimelineEventModel
					{
						ActionName = $"sendDirectMessage_{reachType.ToString()}",
						Data = restModel,
						ExecutionTime = directMessageOptions.ExecutionTime
					}
				};
				return results;
			}
			catch (Exception ee)
			{
				results.IsSuccesful = false;
				results.Info = new ErrorResponse
				{
					Exception = ee,
					Message = ee.Message,
					StatusCode = HttpStatusCode.InternalServerError
				};
				return results;
			}
		}

		public IActionCommit IncludeStrategy(IStrategySettings strategy)
		{
			throw new NotImplementedException();
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
	}
}
