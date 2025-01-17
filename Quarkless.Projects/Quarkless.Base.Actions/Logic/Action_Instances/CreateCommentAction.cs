﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Quarkless.Base.Actions.Models;
using Quarkless.Base.Actions.Models.Enums.ActionTypes;
using Quarkless.Base.Actions.Models.Enums.StrategyType;
using Quarkless.Base.Actions.Models.Factory.Action_Options;
using Quarkless.Base.Actions.Models.Interfaces;
using Quarkless.Base.ContentInfo.Models.Interfaces;
using Quarkless.Base.Heartbeat.Models;
using Quarkless.Base.Heartbeat.Models.Interfaces;
using Quarkless.Base.InstagramComments.Models;
using Quarkless.Base.Lookup.Models.Interfaces;
using Quarkless.Common.Timeline.Models;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Common.Extensions;
using Quarkless.Models.Common.Models;
using Quarkless.Models.Common.Models.Carriers;
using Quarkless.Models.Common.Models.Resolver;
using Quarkless.Models.Common.Models.Topic;

namespace Quarkless.Base.Actions.Logic.Action_Instances
{
	internal class CreateCommentAction : BaseAction, IActionCommit
	{
		public class CreateCommentContainer
		{
			public CTopic Topic { get; set; }
			public CreateCommentRequest CommentRequest { get; set; }
		}

		private readonly IContentInfoBuilder _contentInfoBuilder;
		private readonly IHeartbeatLogic _heartbeatLogic;
		private readonly UserStoreDetails _user;
		private CommentingActionOptions _actionOptions;

		internal CreateCommentAction(UserStoreDetails userStoreDetails, IContentInfoBuilder contentInfoBuilder,
			IHeartbeatLogic heartbeatLogic, ILookupLogic lookupLogic)
			: base(lookupLogic, ActionType.CreateCommentMedia, userStoreDetails)
		{
			_user = userStoreDetails;
			_contentInfoBuilder = contentInfoBuilder;
			_heartbeatLogic = heartbeatLogic;
			_actionOptions = new CommentingActionOptions();
		}
		private async Task<CreateCommentContainer> CommentingByLocation()
		{
			const MetaDataType fetchType = MetaDataType.FetchMediaByUserLocationTargetList;

			var lookups = await GetLookupItems();

			var fetchMedias = (await _heartbeatLogic.GetMetaData<Quarkless.Models.SearchResponse.Media>(new MetaDataFetchRequest
			{
				MetaDataType = fetchType,
				ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
				InstagramId = _user.ShortInstagram.Id,
				AccountId = _user.AccountId
			}))?.Where(s => s.ObjectItem.Medias.Count > 0)
				.SelectMany(_=>_.ObjectItem.Medias)
				.Where(_=> !lookups.Exists(l => l.ObjectId == _.MediaId))
				.ToList();

			var media = fetchMedias?.ElementAtOrDefault(SecureRandom.Next(fetchMedias.Count - 1));

			if (media == null) return null;

			await AddObjectToLookup(media.MediaId);

			return new CreateCommentContainer
			{
				Topic = _user.Profile.ProfileTopic.Category,
				CommentRequest = new CreateCommentRequest
				{
					Media = new MediaShort { 
						Id = media.MediaId,
						MediaUrl = media.MediaUrl.First(),
						CommentCount = int.TryParse(media.CommentCount, out var commentCount) ? commentCount : 0,
						IncludedInMedia = media.PhotosOfI,
						LikesCount = media.LikesCount
					},
					User = new UserShort
					{
						Id = media.User.UserId,
						ProfilePicture = media.User.ProfilePicture,
						Username = media.User.Username
					},
					DataFrom = new DataFrom
					{
						NominatedFrom = fetchType,
						TopicName = media.Topic?.Name
					}
				}
			};
		}
		private async Task<CreateCommentContainer> CommentingByTopic()
		{
			const MetaDataType fetchType = MetaDataType.FetchMediaByCommenters;

			var lookups = await GetLookupItems();

			var fetchMedias = (await _heartbeatLogic.GetMetaData<Quarkless.Models.SearchResponse.Media>(new MetaDataFetchRequest
			{
				MetaDataType = fetchType,
				ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
				InstagramId = _user.ShortInstagram.Id,
				AccountId = _user.AccountId
			}))?.Where(s => s.ObjectItem.Medias.Count > 0)
				.SelectMany(_=>_.ObjectItem.Medias)
				.Where(_=> !lookups.Exists(l => l.ObjectId == _.MediaId))
				.ToList();

			var media = fetchMedias?.ElementAtOrDefault(SecureRandom.Next(fetchMedias.Count - 1));

			if (media == null) return null;

			await AddObjectToLookup(media.MediaId);

			return new CreateCommentContainer
			{
				Topic = _user.Profile.ProfileTopic.Category,
				CommentRequest = new CreateCommentRequest
				{
					Media = new MediaShort
					{
						Id = media.MediaId,
						MediaUrl = media.MediaUrl.First(),
						CommentCount = int.TryParse(media.CommentCount, out var commentCount) ? commentCount : 0,
						IncludedInMedia = media.PhotosOfI,
						LikesCount = media.LikesCount
					},
					User = new UserShort
					{
						Id = media.User.UserId,
						ProfilePicture = media.User.ProfilePicture,
						Username = media.User.Username
					},
					DataFrom = new DataFrom
					{
						NominatedFrom = fetchType,
						TopicName = media.Topic?.Name
					}
				}
			};
		}
		private async Task<CreateCommentContainer> CommentingByLikers()
		{
			const MetaDataType fetchType = MetaDataType.FetchMediaByLikers;
			
			var lookups = await GetLookupItems();

			var fetchMedias = (await _heartbeatLogic.GetMetaData<Quarkless.Models.SearchResponse.Media>(new MetaDataFetchRequest
			{
				MetaDataType = fetchType,
				ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
				InstagramId = _user.ShortInstagram.Id,
				AccountId = _user.AccountId
			}))?.Where(s => s.ObjectItem.Medias.Count > 0)
				.SelectMany(_=>_.ObjectItem.Medias)
				.Where(_=> !lookups.Exists(l => l.ObjectId == _.MediaId))
				.ToList();

			var media = fetchMedias?.ElementAtOrDefault(SecureRandom.Next(fetchMedias.Count - 1));

			if (media == null) return null;

			await AddObjectToLookup(media.MediaId);

			return new CreateCommentContainer
			{
				Topic = _user.Profile.ProfileTopic.Category,
				CommentRequest = new CreateCommentRequest
				{
					Media = new MediaShort
					{
						Id = media.MediaId,
						MediaUrl = media.MediaUrl.First(),
						CommentCount = int.TryParse(media.CommentCount, out var commentCount) ? commentCount : 0,
						IncludedInMedia = media.PhotosOfI,
						LikesCount = media.LikesCount
					},
					User = new UserShort
					{
						Id = media.User.UserId,
						ProfilePicture = media.User.ProfilePicture,
						Username = media.User.Username
					},
					DataFrom = new DataFrom
					{
						NominatedFrom = fetchType,
						TopicName = media.Topic?.Name
					}
				}
			};
		}

		public async Task<ResultCarrier<EventActionModel>> PushAsync(DateTimeOffset executionTime)
		{
			Console.WriteLine($"Create Comment Action Started: {_user.AccountId}, {_user.InstagramAccountUsername}, {_user.InstagramAccountUser}");
			var results = new ResultCarrier<EventActionModel>();

			try
			{
				switch (_actionOptions.StrategySettings.StrategyType)
				{
					case CommentingStrategy.Default:
						var nominatedMedia = new CreateCommentContainer();
						CommentingActionType commentingActionSelected;

						if (_actionOptions.CommentingActionType == CommentingActionType.Any)
						{
							var commentingActionChances = new List<Chance<CommentingActionType>>
							{
								new Chance<CommentingActionType>
								{
									Object = CommentingActionType.CommentingViaTopic, 
									Probability = 0.25
								},
								new Chance<CommentingActionType>
								{
									Object = CommentingActionType.CommentingViaLikersPosts, 
									Probability = 0.40
								},
							};

							if (_user.Profile.LocationTargetList != null)
								if (_user.Profile.LocationTargetList.Count > 0)
									commentingActionChances.Add(new Chance<CommentingActionType>
									{
										Object = CommentingActionType.CommentingViaLocation, 
										Probability = 0.35
									});

							commentingActionSelected = SecureRandom.ProbabilityRoll(commentingActionChances);
						}
						else
						{
								commentingActionSelected = _actionOptions.CommentingActionType;
						}

						switch (commentingActionSelected)
						{
							case CommentingActionType.CommentingViaTopic:
								nominatedMedia = await CommentingByTopic();
								break;
							case CommentingActionType.CommentingViaLikersPosts:
								nominatedMedia = await CommentingByLikers();
								break;
							case CommentingActionType.CommentingViaLocation:
								nominatedMedia = await CommentingByLocation();
								break;
						}

						if (nominatedMedia == null || string.IsNullOrEmpty(nominatedMedia?.CommentRequest.Media.MediaUrl))
						{
							results.IsSuccessful = false;
							results.Info = new ErrorResponse
							{
								Message = $"No nominated media found, user: {_user.AccountId}, instaId: {_user.InstagramAccountUsername}",
								StatusCode = System.Net.HttpStatusCode.NotFound
							};
							return results;
						}

						var @event = new EventActionModel($"Comment_{_actionOptions.StrategySettings.StrategyType.ToString()}_{commentingActionSelected.ToString()}")
						{
							ActionType = ActionType.CreateCommentMedia,
							User = new UserStore
							{
								AccountId = _user.AccountId,
								InstagramAccountUsername = _user.InstagramAccountUsername,
								InstagramAccountUser = _user.InstagramAccountUser
							}
						};

						nominatedMedia.CommentRequest.Text = _contentInfoBuilder.GenerateComment(nominatedMedia.Topic);
						@event.DataObjects.Add(new EventBody(nominatedMedia.CommentRequest, nominatedMedia.CommentRequest.GetType(), executionTime));

						results.IsSuccessful = true;
						results.Results = @event;
						return results;
					case CommentingStrategy.TopNth:
						throw new NotImplementedException();
					default: throw new Exception("Invalid Comment Strategy");
				}
			}
			catch(Exception err)
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
				Console.WriteLine($"Send Comment Action Ended: { _user.AccountId}, { _user.InstagramAccountUsername}, { _user.InstagramAccountUser}");
			}
		}

		public IActionCommit ModifyOptions(IActionOptions newOptions)
		{
			try
			{
				_actionOptions = newOptions as CommentingActionOptions;
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
			}
			return this;
		}
	}
}
