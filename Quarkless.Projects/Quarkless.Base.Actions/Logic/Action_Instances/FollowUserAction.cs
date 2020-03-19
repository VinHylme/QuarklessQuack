using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Quarkless.Base.Actions.Models;
using Quarkless.Base.Actions.Models.Enums.ActionTypes;
using Quarkless.Base.Actions.Models.Factory.Action_Options;
using Quarkless.Base.Actions.Models.Interfaces;
using Quarkless.Base.ContentInfo.Models.Interfaces;
using Quarkless.Base.Heartbeat.Models;
using Quarkless.Base.Heartbeat.Models.Interfaces;
using Quarkless.Base.InstagramSearch.Models;
using Quarkless.Base.InstagramUser.Models;
using Quarkless.Base.Lookup.Models.Interfaces;
using Quarkless.Common.Timeline.Models;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Common.Extensions;
using Quarkless.Models.Common.Models;
using Quarkless.Models.Common.Models.Carriers;
using Quarkless.Models.Common.Models.Resolver;
using Quarkless.Models.SearchResponse;

namespace Quarkless.Base.Actions.Logic.Action_Instances
{
	internal class FollowUserAction : BaseAction, IActionCommit
	{
		private readonly IContentInfoBuilder _contentInfoBuilder;
		private readonly IHeartbeatLogic _heartbeatLogic;
		private readonly UserStoreDetails _user;
		private FollowActionOptions _actionOptions;

		internal FollowUserAction(UserStoreDetails userStoreDetails, IContentInfoBuilder contentInfoBuilder,
			IHeartbeatLogic heartbeatLogic, ILookupLogic lookupLogic)
			: base(lookupLogic, ActionType.FollowUser, userStoreDetails)
		{
			_contentInfoBuilder = contentInfoBuilder;
			_heartbeatLogic = heartbeatLogic;
			_user = userStoreDetails;
			_actionOptions = new FollowActionOptions();
		}

		private async Task<FollowAndUnFollowUserRequest> FollowBasedOnLikers()
		{
			const MetaDataType fetchType = MetaDataType.FetchUsersViaPostLiked;
			var lookups = await GetLookupItems();
			var fetchMedias = (await _heartbeatLogic.GetMetaData<List<UserResponse<string>>>(new MetaDataFetchRequest
			{
				MetaDataType = fetchType,
				ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
				InstagramId = _user.ShortInstagram.Id,
				AccountId = _user.AccountId
			}))?.Where(s => s.ObjectItem.Count > 0)
				.SelectMany(_=>_.ObjectItem)
				.Where(_=> !lookups.Exists(l => l.ObjectId == _.UserId.ToString()))
				.ToList();

			var item = fetchMedias?.ElementAtOrDefault(SecureRandom.Next(fetchMedias.Count - 1));

			if (item == null) return null;

			await AddObjectToLookup(item.UserId.ToString());

			return new FollowAndUnFollowUserRequest
			{
				UserId = item.UserId,
				User = new UserShort
				{
					Id = item.UserId,
					ProfilePicture = item.ProfilePicture,
					Username = item.Username
				},
				DataFrom = new DataFrom
				{
					NominatedFrom = fetchType,
					TopicName = item.Topic?.Name
				}
			};
		}
		private async Task<FollowAndUnFollowUserRequest> FollowBasedOnTopic()
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
				.SelectMany(_=>_.ObjectItem.Medias)
				.Where(_=> !lookups.Exists(l=>l.ObjectId == _.User.UserId.ToString()))
				.ToList();

			var item = fetchMedias?.ElementAtOrDefault(SecureRandom.Next(fetchMedias.Count - 1));
			
			if (item == null) return null;

			await AddObjectToLookup(item.User.UserId.ToString());

			return new FollowAndUnFollowUserRequest
			{
				UserId = item.User.UserId,
				User = new UserShort
				{
					Id = item.User.UserId,
					ProfilePicture = item.User.ProfilePicture,
					Username = item.User.Username
				},
				DataFrom = new DataFrom
				{
					NominatedFrom = fetchType,
					TopicName = item.Topic?.Name
				}
			};
		}
		private async Task<FollowAndUnFollowUserRequest> FollowBasedOnLocation()
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
				.Where(_=>!lookups.Exists(l=>l.ObjectId == _.User.UserId.ToString()))
				.ToList();

			var item = fetchMedias?.ElementAtOrDefault(SecureRandom.Next(fetchMedias.Count - 1));

			if (item == null) return null;

			await AddObjectToLookup(item.User.UserId.ToString());

			return new FollowAndUnFollowUserRequest
			{
				UserId = item.User.UserId,
				User = new UserShort
				{
					Id = item.User.UserId,
					ProfilePicture = item.User.ProfilePicture,
					Username = item.User.Username
				},
				DataFrom = new DataFrom
				{
					NominatedFrom = fetchType,
					TopicName = item.Topic?.Name
				}
			};
		}
		private async Task<FollowAndUnFollowUserRequest> FollowBasedOnCommenters()
		{
			const MetaDataType fetchType = MetaDataType.FetchUsersViaPostCommented;
			var lookups = await GetLookupItems();
			var fetchMedias = (await _heartbeatLogic.GetMetaData<List<UserResponse<CommentResponse>>>(new MetaDataFetchRequest
			{
				MetaDataType = fetchType,
				ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
				InstagramId = _user.ShortInstagram.Id,
				AccountId = _user.AccountId
			}))?.Where(s => s.ObjectItem.Count > 0)
				.SelectMany(_=>_.ObjectItem)
				.Where(_=>!lookups.Exists(l=>l.ObjectId == _.UserId.ToString()))
				.ToList();

			var item = fetchMedias?.ElementAtOrDefault(SecureRandom.Next(fetchMedias.Count - 1));

			if (item == null) return null;

			await AddObjectToLookup(item.UserId.ToString());

			return new FollowAndUnFollowUserRequest
			{
				UserId = item.UserId,
				User = new UserShort
				{
					Id = item.UserId,
					ProfilePicture = item.ProfilePicture,
					Username = item.Username
				},
				DataFrom = new DataFrom
				{
					NominatedFrom = fetchType,
					TopicName = item.Topic?.Name
				}
			};
		}
		private async Task<FollowAndUnFollowUserRequest> FollowBasedOnSuggestions()
		{
			const MetaDataType fetchType = MetaDataType.FetchUsersFollowSuggestions;

			var lookups = await GetLookupItems();

			var fetchMedias = (await _heartbeatLogic.GetMetaData<List<UserResponse<UserSuggestionDetails>>>(new MetaDataFetchRequest
			{
				MetaDataType = fetchType,
				ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
				InstagramId = _user.ShortInstagram.Id,
				AccountId = _user.AccountId
			}))?.Where(s => s.ObjectItem.Count > 0)
				.SelectMany(_=>_.ObjectItem)
				.Where(_=> !lookups.Exists(l => l.ObjectId == _.UserId.ToString()))
				.ToList();

			var item = fetchMedias?.ElementAtOrDefault(SecureRandom.Next(fetchMedias.Count - 1));
			
			if (item == null) return null;

			await AddObjectToLookup(item.UserId.ToString());

			return new FollowAndUnFollowUserRequest
			{
				UserId = item.UserId,
				User = new UserShort
				{
					Id = item.UserId,
					ProfilePicture = item.ProfilePicture,
					Username = item.Username
				},
				DataFrom = new DataFrom
				{
					NominatedFrom = fetchType,
					TopicName = item.Topic?.Name
				}
			};
		}

		public async Task<ResultCarrier<EventActionModel>> PushAsync(DateTimeOffset executionTime)
		{
			Console.WriteLine($"Follow Action Started: {_user.AccountId}, {_user.InstagramAccountUsername}, {_user.InstagramAccountUser}");
			var results = new ResultCarrier<EventActionModel>();
			try
			{
				var followActionTypeSelected = FollowActionType.FollowBasedOnTopic;
				FollowAndUnFollowUserRequest nominatedFollower = null;
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
				if (nominatedFollower == null || nominatedFollower.UserId == 0)
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
				@event.DataObjects.Add(new EventBody(nominatedFollower, nominatedFollower.GetType(), executionTime));
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
