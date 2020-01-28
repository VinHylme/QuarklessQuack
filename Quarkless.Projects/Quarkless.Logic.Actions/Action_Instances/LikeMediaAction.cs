using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Quarkless.Models.Actions;
using Quarkless.Models.Actions.Enums.ActionTypes;
using Quarkless.Models.Actions.Enums.StrategyType;
using Quarkless.Models.Actions.Factory.Action_Options;
using Quarkless.Models.Actions.Interfaces;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Common.Extensions;
using Quarkless.Models.Common.Models;
using Quarkless.Models.Common.Models.Carriers;
using Quarkless.Models.ContentInfo.Interfaces;
using Quarkless.Models.Heartbeat;
using Quarkless.Models.Heartbeat.Interfaces;
using Quarkless.Models.SearchResponse;
using Quarkless.Models.Timeline;
using ActionType = Quarkless.Models.Actions.Enums.ActionType;

namespace Quarkless.Logic.Actions.Action_Instances
{
	internal class LikeMediaAction : IActionCommit
	{
		private readonly IContentInfoBuilder _contentInfoBuilder;
		private readonly IHeartbeatLogic _heartbeatLogic;
		private readonly UserStoreDetails _user;
		private LikeActionOptions _actionOptions;
		internal LikeMediaAction(UserStoreDetails userStoreDetails, IContentInfoBuilder contentInfoBuilder,
			IHeartbeatLogic heartbeatLogic)
		{
			_user = userStoreDetails;
			_contentInfoBuilder = contentInfoBuilder;
			_heartbeatLogic = heartbeatLogic;
			_actionOptions = new LikeActionOptions();
		}
		private async Task<string> LikeUserFeedMedia()
		{
			var by = new By
			{
				ActionType = (int)ActionType.LikePost,
				User = _user.Profile.InstagramAccountId
			};

			var fetchMedias = (await _heartbeatLogic.GetMetaData<Media>(
					new MetaDataFetchRequest
					{
						MetaDataType = MetaDataType.FetchUsersFeed,
						ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
						InstagramId = _user.ShortInstagram.Id
					}))?.Where(exclude => !exclude.SeenBy.Any(e => e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Medias.Count > 0).ToList();

			var select = fetchMedias?.ElementAtOrDefault(SecureRandom.Next(fetchMedias.Count));

			if (select == null) return null;
			select.SeenBy.Add(by);

			await _heartbeatLogic.UpdateMetaData(new MetaDataCommitRequest<Media>
			{
				MetaDataType = MetaDataType.FetchUsersFeed,
				ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
				InstagramId = _user.ShortInstagram.Id,
				Data = select
			});

			return select.ObjectItem?.Medias?.FirstOrDefault()?.MediaId;
		}
		private async Task<string> LikeUsersMediaByTopic()
		{
			var by = new By
			{
				ActionType = (int)ActionType.LikePost,
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
			if (select == null) return null;
			select.SeenBy.Add(by);

			await _heartbeatLogic.UpdateMetaData(new MetaDataCommitRequest<Media>
			{
				MetaDataType = MetaDataType.FetchMediaByTopic,
				ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
				InstagramId = _user.ShortInstagram.Id,
				Data = select
			});

			return select.ObjectItem?.Medias?.FirstOrDefault()?.MediaId;
		}
		private async Task<string> LikeUsersMediaByLocation()
		{
			var by = new By
			{
				ActionType = (int)ActionType.LikePost,
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
			if (select == null) return null;
			select.SeenBy.Add(by);

			await _heartbeatLogic.UpdateMetaData(new MetaDataCommitRequest<Media>
			{
				MetaDataType = MetaDataType.FetchMediaByUserLocationTargetList,
				ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
				InstagramId = _user.ShortInstagram.Id,
				Data = select
			});

			return select.ObjectItem?.Medias?.FirstOrDefault()?.MediaId;
		}
		private async Task<string> LikeUsersMediaLikers()
		{
			var by = new By
			{
				ActionType = (int)ActionType.LikePost,
				User = _user.Profile.InstagramAccountId
			};

			var fetchMedias = (await _heartbeatLogic.GetMetaData<Media>(
			new MetaDataFetchRequest
			{
				MetaDataType = MetaDataType.FetchMediaByLikers,
				ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
				InstagramId = _user.ShortInstagram.Id
			}))?.Where(exclude => !exclude.SeenBy.Any(e => e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Medias.Count > 0).ToList();

			var select = fetchMedias?.ElementAtOrDefault(SecureRandom.Next(fetchMedias.Count));
			if (select == null) return null;
			select.SeenBy.Add(by);

			await _heartbeatLogic.UpdateMetaData(new MetaDataCommitRequest<Media>
			{
				MetaDataType = MetaDataType.FetchMediaByLikers,
				ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
				InstagramId = _user.ShortInstagram.Id,
				Data = select
			});

			return select.ObjectItem?.Medias?.FirstOrDefault()?.MediaId;
		}
		private async Task<string> LikeUsersMediaCommenters()
		{
			var by = new By
			{
				ActionType = (int)ActionType.LikePost,
				User = _user.Profile.InstagramAccountId
			};

			var fetchMedias = (await _heartbeatLogic.GetMetaData<Media>(new MetaDataFetchRequest
			{
				MetaDataType = MetaDataType.FetchMediaByCommenters,
				ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
				InstagramId = _user.ShortInstagram.Id
			}))?.Where(exclude => !exclude.SeenBy.Any(e => e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Medias.Count > 0).ToList();

			var select = fetchMedias?.ElementAtOrDefault(SecureRandom.Next(fetchMedias.Count));
			if (select == null) return null;
			select.SeenBy.Add(by);

			await _heartbeatLogic.UpdateMetaData(new MetaDataCommitRequest<Media>
			{
				MetaDataType = MetaDataType.FetchMediaByCommenters,
				ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
				InstagramId = _user.ShortInstagram.Id,
				Data = select
			});
			return select.ObjectItem?.Medias?.FirstOrDefault()?.MediaId;
		}

		public async Task<ResultCarrier<EventActionModel>> PushAsync(DateTimeOffset executionTime)
		{
			Console.WriteLine($"Like Post Action Started: {_user.OAccountId}, {_user.OInstagramAccountUsername}, {_user.OInstagramAccountUser}");
			var results = new ResultCarrier<EventActionModel>();
			try
			{
				switch (_actionOptions.StrategySettings.StrategyType)
				{
					case LikeStrategyType.Default:
					{
						var nominatedMedia = string.Empty;
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

						if (string.IsNullOrEmpty(nominatedMedia))
						{
							results.IsSuccessful = false;
							results.Info = new ErrorResponse
							{
								Message =
									$"could not find any good media to like, user: {_user.OAccountId}, instaId: {_user.OInstagramAccountUsername}",
								StatusCode = System.Net.HttpStatusCode.NotFound
							};
							return results;
						}

						var @event = new EventActionModel($"LikeMedia_{_actionOptions.StrategySettings.StrategyType.ToString()}_{likeActionTypeSelected.ToString()}")
						{
							ActionType = ActionType.LikePost,
							User = new UserStore
							{
								OAccountId = _user.OAccountId,
								OInstagramAccountUsername = _user.OInstagramAccountUsername,
								OInstagramAccountUser = _user.OInstagramAccountUser
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
								OAccountId = _user.OAccountId,
								OInstagramAccountUsername = _user.OInstagramAccountUsername,
								OInstagramAccountUser = _user.OInstagramAccountUser
							}
						};

						var by = new By
						{
							ActionType = (int) ActionType.LikePost,
							User = _user.Profile.InstagramAccountId
						};

						var fetchMedias = (await _heartbeatLogic.GetMetaData<Media>(new MetaDataFetchRequest
							{
								MetaDataType = MetaDataType.FetchMediaByTopic,
								ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
								InstagramId = _user.ShortInstagram.Id
							}))?.Where(exclude => !exclude.SeenBy.Any(e => e.User == @by.User && e.ActionType == @by.ActionType))
								.ToList();

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
								if (media == null) continue;

								media.SeenBy.Add(@by);

								await _heartbeatLogic.UpdateMetaData(new MetaDataCommitRequest<Media>
								{
									MetaDataType = MetaDataType.FetchUsersFeed,
									ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
									InstagramId = _user.ShortInstagram.Id,
									Data = media,
								});

								var nominatedMedia = media.ObjectItem?.Medias?.FirstOrDefault()?.MediaId;
								if (nominatedMedia == null) continue;

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
				Console.WriteLine($"Like Post Action Ended: {_user.OAccountId}, {_user.OInstagramAccountUsername}, {_user.OInstagramAccountUser}");
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
