using System;
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
using Quarkless.Base.Lookup.Models.Interfaces;
using Quarkless.Base.Media.Models;
using Quarkless.Common.Timeline.Models;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Common.Extensions;
using Quarkless.Models.Common.Models;
using Quarkless.Models.Common.Models.Carriers;
using Quarkless.Models.Common.Models.Resolver;

namespace Quarkless.Base.Actions.Logic.Action_Instances
{
	internal class LikeMediaAction : BaseAction, IActionCommit
	{
		private readonly IContentInfoBuilder _contentInfoBuilder;
		private readonly IHeartbeatLogic _heartbeatLogic;
		private readonly UserStoreDetails _user;
		private LikeActionOptions _actionOptions;
		internal LikeMediaAction(UserStoreDetails userStoreDetails, IContentInfoBuilder contentInfoBuilder,
			IHeartbeatLogic heartbeatLogic, ILookupLogic lookupLogic)
			: base(lookupLogic, ActionType.LikePost, userStoreDetails)
		{
			_user = userStoreDetails;
			_contentInfoBuilder = contentInfoBuilder;
			_heartbeatLogic = heartbeatLogic;
			_actionOptions = new LikeActionOptions();
		}
		private async Task<LikeMediaModel> LikeUserFeedMedia()
		{
			const MetaDataType fetchType = MetaDataType.FetchUsersFeed;
			var lookups = await GetLookupItems();
			var fetchMedias = (await _heartbeatLogic.GetMetaData<Quarkless.Models.SearchResponse.Media>(
			new MetaDataFetchRequest
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

			return new LikeMediaModel
			{
				Media = new MediaShort
				{
					Id = media.MediaId,
					MediaUrl = media.MediaUrl.First(),
					CommentCount = int.TryParse(media.CommentCount, out var count) ? count : 0,
					LikesCount = media.LikesCount,
					IncludedInMedia = media.PhotosOfI
				},
				User = new UserShort
				{
					Id = media.User.UserId,
					Username = media.User.Username,
					ProfilePicture = media.User.ProfilePicture
				},
				DataFrom = new DataFrom
				{
					NominatedFrom = fetchType,
					TopicName = media.Topic?.Name
				}
			};
		}
		private async Task<LikeMediaModel> LikeUsersMediaByTopic()
		{
			const MetaDataType fetchType = MetaDataType.FetchMediaByTopic;
			var lookups = await GetLookupItems();
			var fetchMedias = (await _heartbeatLogic.GetMetaData<Quarkless.Models.SearchResponse.Media>(new MetaDataFetchRequest
			{
				MetaDataType = fetchType,
				ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
				InstagramId = _user.ShortInstagram.Id,
				AccountId = _user.AccountId
			}))?.Where(s => s.ObjectItem.Medias.Count > 0)
				.SelectMany(_ => _.ObjectItem.Medias)
				.Where(_ => !lookups.Exists(l => l.ObjectId == _.MediaId))
				.ToList();

			var media = fetchMedias?.ElementAtOrDefault(SecureRandom.Next(fetchMedias.Count - 1));
			
			if (media == null) return null;

			await AddObjectToLookup(media.MediaId);

			return new LikeMediaModel
			{
				Media = new MediaShort
				{
					Id = media.MediaId,
					MediaUrl = media.MediaUrl.First(),
					CommentCount = int.TryParse(media.CommentCount, out var count) ? count : 0,
					LikesCount = media.LikesCount,
					IncludedInMedia = media.PhotosOfI
				},
				User = new UserShort
				{
					Id = media.User.UserId,
					Username = media.User.Username,
					ProfilePicture = media.User.ProfilePicture
				},
				DataFrom = new DataFrom
				{
					NominatedFrom = fetchType,
					TopicName = media.Topic?.Name
				}
			};
		}
		private async Task<LikeMediaModel> LikeUsersMediaByLocation()
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
				.SelectMany(_ => _.ObjectItem.Medias)
				.Where(_ => !lookups.Exists(l => l.ObjectId == _.MediaId))
				.ToList();

			var media = fetchMedias?.ElementAtOrDefault(SecureRandom.Next(fetchMedias.Count - 1));

			if (media == null) return null;

			await AddObjectToLookup(media.MediaId);
			return new LikeMediaModel
			{
				Media = new MediaShort
				{
					Id = media.MediaId,
					MediaUrl = media.MediaUrl.First(),
					CommentCount = int.TryParse(media.CommentCount, out var count) ? count : 0,
					LikesCount = media.LikesCount,
					IncludedInMedia = media.PhotosOfI
				},
				User = new UserShort
				{
					Id = media.User.UserId,
					Username = media.User.Username,
					ProfilePicture = media.User.ProfilePicture
				},
				DataFrom = new DataFrom
				{
					NominatedFrom = fetchType,
					TopicName = media.Topic?.Name
				}
			};
		}
		private async Task<LikeMediaModel> LikeUsersMediaLikers()
		{
			const MetaDataType fetchType = MetaDataType.FetchMediaByLikers;
			var lookups = await GetLookupItems();
			var fetchMedias = (await _heartbeatLogic.GetMetaData<Quarkless.Models.SearchResponse.Media>(
			new MetaDataFetchRequest
			{
				MetaDataType = fetchType,
				ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
				InstagramId = _user.ShortInstagram.Id,
				AccountId = _user.AccountId
			}))?.Where(s => s.ObjectItem.Medias.Count > 0)
				.SelectMany(_ => _.ObjectItem.Medias)
				.Where(_ => !lookups.Exists(l => l.ObjectId == _.MediaId))
				.ToList();

			var media = fetchMedias?.ElementAtOrDefault(SecureRandom.Next(fetchMedias.Count - 1));

			if (media == null) return null;
			await AddObjectToLookup(media.MediaId);
			return new LikeMediaModel
			{
				Media = new MediaShort
				{
					Id = media.MediaId,
					MediaUrl = media.MediaUrl.First(),
					CommentCount = int.TryParse(media.CommentCount, out var count) ? count : 0,
					LikesCount = media.LikesCount,
					IncludedInMedia = media.PhotosOfI
				},
				User = new UserShort
				{
					Id = media.User.UserId,
					Username = media.User.Username,
					ProfilePicture = media.User.ProfilePicture
				},
				DataFrom = new DataFrom
				{
					NominatedFrom = fetchType,
					TopicName = media.Topic?.Name
				}
			};
		}
		private async Task<LikeMediaModel> LikeUsersMediaCommenters()
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
				.SelectMany(_ => _.ObjectItem.Medias)
				.Where(_ => !lookups.Exists(l => l.ObjectId == _.MediaId))
				.ToList();

			var media = fetchMedias?.ElementAtOrDefault(SecureRandom.Next(fetchMedias.Count - 1));

			if (media == null) return null;
			await AddObjectToLookup(media.MediaId);
			return new LikeMediaModel
			{
				Media = new MediaShort
				{
					Id = media.MediaId,
					MediaUrl = media.MediaUrl.First(),
					CommentCount = int.TryParse(media.CommentCount, out var count) ? count : 0,
					LikesCount = media.LikesCount,
					IncludedInMedia = media.PhotosOfI
				},
				User = new UserShort
				{
					Id = media.User.UserId,
					Username = media.User.Username,
					ProfilePicture = media.User.ProfilePicture
				},
				DataFrom = new DataFrom
				{
					NominatedFrom = fetchType,
					TopicName = media.Topic?.Name
				}
			};
		}

		public async Task<ResultCarrier<EventActionModel>> PushAsync(DateTimeOffset executionTime)
		{
			Console.WriteLine($"Like Post Action Started: {_user.AccountId}, {_user.InstagramAccountUsername}, {_user.InstagramAccountUser}");
			var results = new ResultCarrier<EventActionModel>();
			try
			{
				switch (_actionOptions.StrategySettings.StrategyType)
				{
					case LikeStrategyType.Default:
					{
						LikeMediaModel nominatedMedia = null;
						LikeActionType likeActionTypeSelected;
						if (_actionOptions.LikeActionType == LikeActionType.Any)
						{
							var likeActionsChances = new List<Chance<LikeActionType>>();

							if (_user.Profile.LocationTargetList != null)
							{
								if (_user.Profile.LocationTargetList.Count > 0)
								{
									likeActionsChances.AddRange(new List<Chance<LikeActionType>>
									{
										new Chance<LikeActionType>
										{
											Object = LikeActionType.LikeByTopic,
											Probability = 0.10
										},
										new Chance<LikeActionType>
										{
											Object = LikeActionType.LikeFromUsersFeed,
											Probability = _user.Profile.AdditionalConfigurations.FocusLocalMore
												? 0.15
												: 0.30
										},
										new Chance<LikeActionType>
										{
											Object = LikeActionType.LikeUsersMediaByCommenters,
											Probability = 0.15
										},
										new Chance<LikeActionType>
										{
											Object = LikeActionType.LikeUsersMediaByLikers,
											Probability = 0.30
										},
										new Chance<LikeActionType>
										{
											Object = LikeActionType.LikeUsersMediaByLocation,
											Probability = _user.Profile.AdditionalConfigurations.FocusLocalMore
												? 0.30
												: 0.15
										}
									});
								}
								else
								{
									likeActionsChances.AddRange(new List<Chance<LikeActionType>>
									{
										new Chance<LikeActionType>
										{
											Object = LikeActionType.LikeByTopic,
											Probability = 0.20
										},
										new Chance<LikeActionType>
										{
											Object = LikeActionType.LikeFromUsersFeed,
											Probability = 0.30
										},
										new Chance<LikeActionType>
										{
											Object = LikeActionType.LikeUsersMediaByCommenters,
											Probability = 0.20
										},
										new Chance<LikeActionType>
										{
											Object = LikeActionType.LikeUsersMediaByLikers,
											Probability = 0.30
										}
									});
								}
							}

							likeActionTypeSelected = SecureRandom.ProbabilityRoll(likeActionsChances);
						}
						else
						{
							likeActionTypeSelected = _actionOptions.LikeActionType;
						}

						switch (likeActionTypeSelected)
						{
							case LikeActionType.LikeByTopic:
								nominatedMedia = await LikeUsersMediaByTopic();
								break;
							case LikeActionType.LikeFromUsersFeed:
								nominatedMedia = await LikeUserFeedMedia();
								break;
							case LikeActionType.LikeUsersMediaByCommenters:
								nominatedMedia = await LikeUsersMediaCommenters();
								break;
							case LikeActionType.LikeUsersMediaByLikers:
								nominatedMedia = await LikeUsersMediaLikers();
								break;
							case LikeActionType.LikeUsersMediaByLocation:
								nominatedMedia = await LikeUsersMediaByLocation();
								break;
						}

						if (nominatedMedia == null)
						{
							results.IsSuccessful = false;
							results.Info = new ErrorResponse
							{
								Message =
									$"could not find any good media to like, user: {_user.AccountId}, instaId: {_user.InstagramAccountUsername}",
								StatusCode = System.Net.HttpStatusCode.NotFound
							};
							return results;
						}

						var @event = new EventActionModel($"LikeMedia_{_actionOptions.StrategySettings.StrategyType.ToString()}_{likeActionTypeSelected.ToString()}")
						{
							ActionType = ActionType.LikePost,
							User = new UserStore
							{
								AccountId = _user.AccountId,
								InstagramAccountUsername = _user.InstagramAccountUsername,
								InstagramAccountUser = _user.InstagramAccountUser
							}
						};

						@event.DataObjects.Add(new EventBody(nominatedMedia, nominatedMedia.GetType(), executionTime));

						results.IsSuccessful = true;
						results.Results = @event;
						return results;
					}
					case LikeStrategyType.TwoDollarCent:
					{
						var @event = new EventActionModel($"LikeMedia_{_actionOptions.StrategySettings.StrategyType.ToString()}_{_actionOptions.LikeActionType.ToString()}")
						{
							ActionType = ActionType.LikePost,
							User = new UserStore
							{
								AccountId = _user.AccountId,
								InstagramAccountUsername = _user.InstagramAccountUsername,
								InstagramAccountUser = _user.InstagramAccountUser
							}
						};
						var fetchMedias = (await _heartbeatLogic.GetMetaData<Quarkless.Models.SearchResponse.Media>(new MetaDataFetchRequest
							{
								MetaDataType = MetaDataType.FetchMediaByTopic,
								ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
								InstagramId = _user.ShortInstagram.Id,
								AccountId = _user.AccountId
						}))?.ToList();

						if (fetchMedias == null)
						{
							results.IsSuccessful = false;
							results.Info = new ErrorResponse
							{
								Message = "@LikeMediaAction-TwoDollarCent: No Medias were fetched"
							};
							return results;
						}

						var timerCounter = 0;
						var grouped = fetchMedias.GroupBy(a => a.ObjectItem.Medias.FirstOrDefault()?.Topic);

						foreach (var topic in grouped)
						{
							for (var i = 0; i < _actionOptions.StrategySettings.NumberOfActions; i++)
							{
								var media = topic.ElementAtOrDefault(i);

								var nominatedMedia = media?.ObjectItem?.Medias?.FirstOrDefault()?.MediaId;
								if (nominatedMedia == null) continue;

								await AddObjectToLookup(nominatedMedia);
								
								@event.DataObjects.Add(new EventBody(nominatedMedia, nominatedMedia.GetType(), 
								executionTime.AddMinutes(
									_actionOptions.StrategySettings.OffsetPerAction.TotalMinutes * timerCounter++)));
							}
						}

						results.IsSuccessful = true;
						results.Results = @event;
						return results;
					}
					default: throw new Exception("Invalid Strategy Type Selected");
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
				Console.WriteLine($"Like Post Action Ended: {_user.AccountId}, {_user.InstagramAccountUsername}, {_user.InstagramAccountUser}");
			}
		}

		public IActionCommit ModifyOptions(IActionOptions newOptions)
		{
			try
			{
				_actionOptions = newOptions as LikeActionOptions;
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
			}
			return this;
		}
	}
}
