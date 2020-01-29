using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Quarkless.Base.InstagramUser.Models;
using Quarkless.Models.Actions;
using Quarkless.Models.Actions.Enums.ActionTypes;
using Quarkless.Models.Actions.Factory.Action_Options;
using Quarkless.Models.Actions.Interfaces;
using Quarkless.Models.Actions.Models;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Common.Extensions;
using Quarkless.Models.Common.Models;
using Quarkless.Models.Common.Models.Carriers;
using Quarkless.Models.ContentInfo.Interfaces;
using Quarkless.Models.Heartbeat;
using Quarkless.Models.Heartbeat.Interfaces;
using Quarkless.Models.InstagramSearch;
using Quarkless.Models.SearchResponse;

namespace Quarkless.Logic.Actions.Action_Instances
{
	internal class FollowUserAction : IActionCommit
	{
		private readonly IContentInfoBuilder _contentInfoBuilder;
		private readonly IHeartbeatLogic _heartbeatLogic;
		private readonly UserStoreDetails _user;
		private FollowActionOptions _actionOptions;

		internal FollowUserAction(UserStoreDetails userStoreDetails, IContentInfoBuilder contentInfoBuilder, IHeartbeatLogic heartbeatLogic)
		{
			_contentInfoBuilder = contentInfoBuilder;
			_heartbeatLogic = heartbeatLogic;
			_user = userStoreDetails;
			_actionOptions = new FollowActionOptions();
		}

		private async Task<long> FollowBasedOnLikers()
		{
			var by = new By
			{
				ActionType = (int)ActionType.FollowUser,
				User = _user.Profile.InstagramAccountId
			};
			var fetchMedias = (await _heartbeatLogic.GetMetaData<List<UserResponse<string>>>
					(new MetaDataFetchRequest
					{
						MetaDataType = MetaDataType.FetchUsersViaPostLiked,
						ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
						InstagramId = _user.ShortInstagram.Id
					}))?.Where(exclude => !exclude.SeenBy.Any(e => e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Count > 0).ToList();

			var select = fetchMedias?.ElementAtOrDefault(SecureRandom.Next(fetchMedias.Count));
			if (select == null) return 0;

			select.SeenBy.Add(by);

			await _heartbeatLogic.UpdateMetaData(new MetaDataCommitRequest<List<UserResponse<string>>>
			{
				MetaDataType = MetaDataType.FetchUsersViaPostLiked,
				ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
				InstagramId = _user.ShortInstagram.Id,
				Data = select
			});

			return @select.ObjectItem?.ElementAtOrDefault(@select.ObjectItem.Count)?.UserId ?? 0;
		}
		private async Task<long> FollowBasedOnTopic()
		{
			var by = new By
			{
				ActionType = (int)ActionType.FollowUser,
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

			return select.ObjectItem?.Medias.FirstOrDefault()?.User.UserId ?? 0;
		}
		private async Task<long> FollowBasedOnLocation()
		{
			var by = new By
			{
				ActionType = (int)ActionType.FollowUser,
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

			return select.ObjectItem?.Medias?.FirstOrDefault()?.User.UserId ?? 0;
		}
		private async Task<long> FollowBasedOnCommenters()
		{
			var by = new By
			{
				ActionType = (int)ActionType.FollowUser,
				User = _user.Profile.InstagramAccountId
			};

			var fetchMedias = (await _heartbeatLogic.GetMetaData<List<UserResponse<CommentResponse>>>(new MetaDataFetchRequest
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

			return select.ObjectItem?.ElementAtOrDefault(@select.ObjectItem.Count)?.UserId ?? 0;
		}
		private async Task<long> FollowBasedOnSuggestions()
		{
			var by = new By
			{
				ActionType = (int)ActionType.FollowUser,
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

			return select.ObjectItem?.ElementAtOrDefault(select.ObjectItem.Count)?.UserId ?? 0;
		}

		public async Task<ResultCarrier<EventActionModel>> PushAsync(DateTimeOffset executionTime)
		{
			Console.WriteLine($"Follow Action Started: {_user.AccountId}, {_user.InstagramAccountUsername}, {_user.InstagramAccountUser}");
			var results = new ResultCarrier<EventActionModel>();
			try
			{
				var followActionTypeSelected = FollowActionType.FollowBasedOnTopic;
				long nominatedFollower = 0;
				//todo add Location?
				var followActionsChances = new List<Chance<FollowActionType>>();
				if (_actionOptions.FollowActionType == FollowActionType.Any)
				{
					if (_user.Profile.LocationTargetList != null && _user.Profile?.LocationTargetList?.Count > 0)
					{
						followActionsChances.AddRange(new List<Chance<FollowActionType>>
						{
							new Chance<FollowActionType>
							{
								Object = FollowActionType.FollowBasedOnCommenters,
								Probability = 0.20
							},
							new Chance<FollowActionType>
							{
								Object = FollowActionType.FollowBasedOnLocation,
								Probability = _user.Profile.AdditionalConfigurations.FocusLocalMore ? 0.40 : 0.15
							},
							new Chance<FollowActionType>
							{
								Object = FollowActionType.FollowBasedOnLikers,
								Probability = _user.Profile.AdditionalConfigurations.FocusLocalMore ? 0.15 : 0.40
							},
							new Chance<FollowActionType>
							{
								Object = FollowActionType.FollowBasedOnTopic,
								Probability = 0.5
							},
							new Chance<FollowActionType>
							{
								Object = FollowActionType.FollowBasedOnSuggestions,
								Probability = 0.20
							}
						});
					}
					else
					{
						followActionsChances.AddRange(new List<Chance<FollowActionType>>
						{
							new Chance<FollowActionType>
							{
								Object = FollowActionType.FollowBasedOnCommenters,
								Probability = 0.25
							},
							new Chance<FollowActionType>
							{
								Object = FollowActionType.FollowBasedOnLikers,
								Probability = 0.40
							},
							new Chance<FollowActionType>
							{
								Object = FollowActionType.FollowBasedOnTopic,
								Probability = 0.15
							},
							new Chance<FollowActionType>
							{
								Object = FollowActionType.FollowBasedOnSuggestions,
								Probability = 0.20
							}
						});
					}

					followActionTypeSelected = SecureRandom.ProbabilityRoll(followActionsChances);
				}
				else
				{
					if (_actionOptions != null) followActionTypeSelected = _actionOptions.FollowActionType;
				}
				switch (followActionTypeSelected)
				{
					case FollowActionType.FollowBasedOnCommenters:
						nominatedFollower = await FollowBasedOnCommenters();
						break;
					case FollowActionType.FollowBasedOnLikers:
						nominatedFollower = await FollowBasedOnLikers();
						break;
					case FollowActionType.FollowBasedOnTopic:
						nominatedFollower = await FollowBasedOnTopic();
						break;
					case FollowActionType.FollowBasedOnLocation:
						nominatedFollower = await FollowBasedOnLocation();
						break;
					case FollowActionType.FollowBasedOnSuggestions:
						nominatedFollower = await FollowBasedOnSuggestions();
						break;
				}
				if (nominatedFollower == 0)
				{
					results.IsSuccessful = false;
					results.Info = new ErrorResponse
					{
						Message = $"could not find a nominated person to follow, user: {_user.AccountId}, instaId: {_user.InstagramAccountUsername}",
						StatusCode = System.Net.HttpStatusCode.NotFound
					};
					return results;
				}

				var @event = new EventActionModel($"FollowUser_{_actionOptions.StrategySettings.StrategyType.ToString()}_{followActionTypeSelected.ToString()}")
				{
					ActionType = ActionType.FollowUser,
					User = new UserStore
					{
						AccountId = _user.AccountId,
						InstagramAccountUsername = _user.InstagramAccountUsername,
						InstagramAccountUser = _user.InstagramAccountUser
					}
				};
				var request = new FollowAndUnFollowUserRequest
				{
					UserId = nominatedFollower
				};
				@event.DataObjects.Add(new EventBody(request, request.GetType(), executionTime));
				results.Results = @event;
				results.IsSuccessful = true;
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
				Console.WriteLine($"Follow Action End: {_user.AccountId}, {_user.InstagramAccountUsername}, {_user.InstagramAccountUser}");
			}
		}

		public IActionCommit ModifyOptions(IActionOptions newOptions)
		{
			try
			{
				_actionOptions = newOptions as FollowActionOptions;
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
			}
			return this;
		}

	}
}
