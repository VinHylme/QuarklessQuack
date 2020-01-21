using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using InstagramApiSharp.Classes.Models;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Common.Extensions;
using Quarkless.Models.Common.Models;
using Quarkless.Models.Common.Models.Carriers;
using Quarkless.Models.ContentInfo.Interfaces;
using Quarkless.Models.Heartbeat;
using Quarkless.Models.Heartbeat.Interfaces;
using Quarkless.Models.InstagramSearch;
using Quarkless.Models.Messaging;
using Quarkless.Models.RequestBuilder.Interfaces;
using Quarkless.Models.SearchResponse;
using Quarkless.Models.Services.Automation.Enums.Actions.ActionType;
using Quarkless.Models.Services.Automation.Interfaces;
using Quarkless.Models.Services.Automation.Models.ActionOptions;
using Quarkless.Models.Storage.Interfaces;
using Quarkless.Models.Timeline;

namespace Quarkless.Logic.Services.Automation.Actions.EngageActions
{
	public class DirectMessagingAction : IActionCommit
	{
		private readonly IContentInfoBuilder _builder;
		private readonly IHeartbeatLogic _heartbeatLogic;
		private readonly IUrlReader _urlReader;
		private UserStoreDetails user;
		public DirectMessagingAction(IContentInfoBuilder contentManager, IHeartbeatLogic heartbeatLogic, IUrlReader urlReader)
		{
			_heartbeatLogic = heartbeatLogic;
			_builder = contentManager;
			_urlReader = urlReader;
		}

		#region Functionality for each actions
		private long SendDirectMessageBasedOnLikers()
		{
			var by = new By
			{
				ActionType = (int)ActionType.SendDirectMessage,
				User = user.Profile.InstagramAccountId
			};
			var fetchMedias = _heartbeatLogic.GetMetaData<List<UserResponse<string>>>(new MetaDataFetchRequest
				{
					MetaDataType = MetaDataType.FetchUsersViaPostLiked,
					ProfileCategoryTopicId = user.Profile.ProfileTopic.Category._id,
					InstagramId = user.ShortInstagram.Id
				})

				.GetAwaiter().GetResult().Where(exclude=>!exclude.SeenBy.Any(e=>e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Count > 0);
			var meta_S = fetchMedias as Meta<List<UserResponse<string>>>[] ?? fetchMedias.ToArray();
			var select = meta_S.ElementAtOrDefault(SecureRandom.Next(meta_S.Count()));
			if (@select == null) return 0;
			@select.SeenBy.Add(@by);
			_heartbeatLogic.UpdateMetaData(new MetaDataCommitRequest<List<UserResponse<string>>>
			{
				MetaDataType = MetaDataType.FetchUsersViaPostLiked,
				ProfileCategoryTopicId = user.Profile.ProfileTopic.Category._id,
				InstagramId = user.ShortInstagram.Id,
				Data = select
			}).GetAwaiter().GetResult();

			return @select.ObjectItem?.ElementAtOrDefault(@select.ObjectItem.Count)?.UserId ?? 0;
		}
		private long SendDirectMessageBasedOnCommenters()
		{
			var by = new By
			{
				ActionType = (int)ActionType.SendDirectMessage,
				User = user.Profile.InstagramAccountId
			};
			var fetchMedias = _heartbeatLogic.GetMetaData<List<UserResponse<CommentResponse>>>(
					new MetaDataFetchRequest
					{
						MetaDataType = MetaDataType.FetchUsersViaPostCommented,
						ProfileCategoryTopicId = user.Profile.ProfileTopic.Category._id,
						InstagramId = user.ShortInstagram.Id
					})
				.GetAwaiter().GetResult().Where(exclude=>!exclude.SeenBy.Any(e=>e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Count > 0);

			var metaS = fetchMedias as Meta<List<UserResponse<CommentResponse>>>[] ?? fetchMedias.ToArray();
			var select = metaS.ElementAtOrDefault(SecureRandom.Next(metaS.Length));
			if (select == null) return 0;
			select.SeenBy.Add(by);

			_heartbeatLogic.UpdateMetaData(new MetaDataCommitRequest<List<UserResponse<CommentResponse>>>
			{
				MetaDataType = MetaDataType.FetchUsersViaPostCommented,
				ProfileCategoryTopicId = user.Profile.ProfileTopic.Category._id,
				InstagramId = user.ShortInstagram.Id,
				Data = select
			}).GetAwaiter().GetResult();

			return @select.ObjectItem.First().UserId;
		}
		private long SendDirectMessageBasedOnTopic()
		{
			var by = new By
			{
				ActionType = (int)ActionType.SendDirectMessage,
				User = user.Profile.InstagramAccountId
			};
			var fetchMedias = _heartbeatLogic.GetMetaData<Media>(new MetaDataFetchRequest
				{
					MetaDataType = MetaDataType.FetchMediaByTopic,
					ProfileCategoryTopicId = user.Profile.ProfileTopic.Category._id,
					InstagramId = user.ShortInstagram.Id
				})
				.GetAwaiter().GetResult().Where(exclude=>!exclude.SeenBy.Any(e=>e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Medias.Count > 0);

			var metaS = fetchMedias as Meta<Media>[] ?? fetchMedias.ToArray();
			var select = metaS.ElementAtOrDefault(SecureRandom.Next(metaS.Count()));
			if (select == null) return 0;
			select.SeenBy.Add(by);

			_heartbeatLogic.UpdateMetaData(new MetaDataCommitRequest<Media>
			{
				MetaDataType = MetaDataType.FetchMediaByTopic,
				ProfileCategoryTopicId = user.Profile.ProfileTopic.Category._id,
				InstagramId = user.ShortInstagram.Id,
				Data = select
			}).GetAwaiter().GetResult();

			return @select.ObjectItem?.Medias.FirstOrDefault()?.User.UserId ?? 0;
		}
		private long SendDirectMessageBasedOnLocation()
		{
			var by = new By
			{
				ActionType = (int)ActionType.SendDirectMessage,
				User = user.Profile.InstagramAccountId
			};
			var fetchMedias = _heartbeatLogic.GetMetaData<Media>(new MetaDataFetchRequest
				{
					MetaDataType = MetaDataType.FetchMediaByUserLocationTargetList,
					ProfileCategoryTopicId = user.Profile.ProfileTopic.Category._id, 
					InstagramId = user.ShortInstagram.Id
				})
				.GetAwaiter().GetResult()
				.Where(exclude=>!exclude.SeenBy.Any(e=>e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Medias.Count > 0);
			
			var metaS = fetchMedias as Meta<Media>[] ?? fetchMedias.ToArray();
			var select = metaS.ElementAtOrDefault(SecureRandom.Next(metaS.Count()));
			if (select == null) return 0;
			select.SeenBy.Add(by);

			_heartbeatLogic.UpdateMetaData(new MetaDataCommitRequest<Media>
				{
					MetaDataType = MetaDataType.FetchMediaByUserLocationTargetList,
					ProfileCategoryTopicId = user.Profile.ProfileTopic.Category._id,
					InstagramId = user.ShortInstagram.Id,
					Data = select
				})
				.GetAwaiter().GetResult();

			return @select.ObjectItem.Medias.FirstOrDefault()?.User.UserId ?? 0;
		}
		private long SendDirectMessageBasedOnSuggestions()
		{
			var by = new By
			{
				ActionType = (int)ActionType.SendDirectMessage,
				User = user.Profile.InstagramAccountId
			};
			var fetchMedias = _heartbeatLogic.GetMetaData<List<UserResponse<UserSuggestionDetails>>>(new MetaDataFetchRequest
				{
					MetaDataType = MetaDataType.FetchUsersFollowSuggestions,
					ProfileCategoryTopicId = user.Profile.ProfileTopic.Category._id,
					InstagramId = user.ShortInstagram.Id
				})
				.GetAwaiter().GetResult()
				.Where(exclude=>!exclude.SeenBy.Any(e=>e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Count > 0);

			var metaS = fetchMedias as Meta<List<UserResponse<UserSuggestionDetails>>>[] ?? fetchMedias.ToArray();
			var select = metaS.ElementAtOrDefault(SecureRandom.Next(metaS.Count()));
			if (select == null) return 0;
			select.SeenBy.Add(by);

			_heartbeatLogic.UpdateMetaData(new MetaDataCommitRequest<List<UserResponse<UserSuggestionDetails>>>
			{
				MetaDataType = MetaDataType.FetchUsersFollowSuggestions,
				ProfileCategoryTopicId = user.Profile.ProfileTopic.Category._id,
				InstagramId = user.ShortInstagram.Id,
				Data = select
			}).GetAwaiter().GetResult();
			return select.ObjectItem.First().UserId;
		}
		private long SendDirectMessageBasedOnUsersFollowers()
		{
			var by = new By
			{
				ActionType = (int)ActionType.SendDirectMessage,
				User = user.Profile.InstagramAccountId
			};
			var fetchMedias = _heartbeatLogic.GetMetaData<List<UserResponse<string>>>(new MetaDataFetchRequest
				{
					MetaDataType = MetaDataType.FetchUsersFollowerList,
					ProfileCategoryTopicId = user.Profile.ProfileTopic.Category._id,
					InstagramId = user.ShortInstagram.Id
				})
				.GetAwaiter().GetResult().Where(exclude=>!exclude.SeenBy.Any(e=>e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Count > 0);

			var metaS = fetchMedias as Meta<List<UserResponse<string>>>[] ?? fetchMedias.ToArray();
			var select = metaS.ElementAtOrDefault(SecureRandom.Next(metaS.Count()));
			if (select == null) return 0;
			select.SeenBy.Add(by);

			_heartbeatLogic.UpdateMetaData(new MetaDataCommitRequest<List<UserResponse<string>>>
			{
				MetaDataType = MetaDataType.FetchUsersFollowerList,
				ProfileCategoryTopicId = user.Profile.ProfileTopic.Category._id,
				InstagramId = user.ShortInstagram.Id,
				Data = select
			}).GetAwaiter().GetResult();
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
					results.IsSuccessful = false;
					results.Info = new ErrorResponse
					{
						Message = $"user is null, user: {user.OAccountId}, instaId: {user.OInstagramAccountUsername}",
						StatusCode = HttpStatusCode.NotFound
					};
					return results;
				}

				if (!user.Profile.AdditionalConfigurations.EnableAutoDirectMessaging) return results;
				if (user.MessagesTemplates == null || !user.MessagesTemplates.Any())
				{
					results.IsSuccessful = false;
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
					results.IsSuccessful = false;
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
					RequestType = RequestType.Post,
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
						restModel.BaseUrl = _urlReader.SendDirectMessageText;
						restModel.JsonBody = textModel.ToJsonString();
						break;
					case MessageActionType.Link:
						var linkModel = new SendDirectLinkModel
						{
							Recipients = nominatedIds,
							Link = templateSelected?.Entity.Link,
							TextMessage = templateSelected?.Entity.Message
						};
						restModel.BaseUrl = _urlReader.SendDirectMessageLink;
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
						restModel.BaseUrl = _urlReader.SendDirectMessagePhoto;
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
									ImageBytes = directMessageOptions.PostAnalyser.Manipulation.VideoEditor
										.GenerateVideoThumbnail(Convert.FromBase64String(templateSelected?.Entity.MediaBytes.Split(',')[1]))
								}
							}
						};
						restModel.BaseUrl = _urlReader.SendDirectMessageVideo;
						restModel.JsonBody = videoModel.ToJsonString();
						break;
					case MessageActionType.Profile:
						break;
				}

				if (string.IsNullOrEmpty(restModel.BaseUrl) || string.IsNullOrEmpty(restModel.JsonBody))
				{
					results.IsSuccessful = false;
					results.Info = new ErrorResponse
					{
						Message = "Failed at serializing send message type"
					};
					return results;
				}

				results.IsSuccessful = true;
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
				results.IsSuccessful = false;
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
