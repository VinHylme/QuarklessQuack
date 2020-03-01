using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InstagramApiSharp.Classes.Models;
using Quarkless.Models.Actions.Enums.ActionTypes;
using Quarkless.Models.Actions.Models;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Common.Extensions;
using Quarkless.Models.Common.Models.Resolver;
using Quarkless.Models.Heartbeat;
using Quarkless.Models.Heartbeat.Interfaces;
using Quarkless.Models.InstagramSearch;
using Quarkless.Models.Lookup.Interfaces;
using Quarkless.Models.SearchResponse;
using Quarkless.Models.Stories;

namespace Quarkless.Logic.Actions.Action_Instances
{
	public class StoryActionFetcher : BaseAction
	{
		private readonly IHeartbeatLogic _heartbeatLogic;
		private readonly UserStoreDetails _user;

		public StoryActionFetcher(IHeartbeatLogic heartbeatLogic, ILookupLogic lookupLogic,
			ActionType actionType, UserStoreDetails user)
			:base(lookupLogic, actionType, user)
		{
			_heartbeatLogic = heartbeatLogic;
			_user = user;
		}

		#region Getter From heartbeat Logic
		internal async Task<StoryRequest> GetStoryFromLocationTarget()
		{
			const MetaDataType fetchType = MetaDataType.FetchMediaByUserLocationTargetList;
			var lookups = await GetLookupItems();
			var fetchUsers = (await _heartbeatLogic.GetMetaData<Media>(
			new MetaDataFetchRequest
			{
				MetaDataType = fetchType,
				ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
				InstagramId = _user.ShortInstagram.Id,
				AccountId = _user.AccountId
			}))?.Where(s => s.ObjectItem.Medias.Count > 0)
				.SelectMany(_=>_.ObjectItem.Medias)
				.Where(_=> !lookups.Exists(l=>l.ObjectId == _.User.UserId.ToString()))
				.ToList();

			var userResponse = fetchUsers?.ElementAtOrDefault(SecureRandom.Next(fetchUsers.Count - 1));

			if (userResponse == null)
				return null;

			if (userResponse.User.UserId == 0 || userResponse.User.IsPrivate)
				return null;

			await AddObjectToLookup(userResponse.User.UserId.ToString());

			return new StoryRequest
			{
				UserId = userResponse.User.UserId,
				StoryId = null,
				ContainsItems = false,
				Items = null,
				Media = new MediaShort
				{
					Id = userResponse.MediaId,
					MediaUrl = userResponse.MediaUrl.First(),
					CommentCount = int.TryParse(userResponse.CommentCount, out var count) ? count : 0,
					LikesCount = userResponse.LikesCount,
					IncludedInMedia = userResponse.PhotosOfI
				},
				User = new UserShort
				{
					Id = userResponse.User.UserId,
					Username = userResponse.User.Username,
					ProfilePicture = userResponse.User.ProfilePicture
				},
				DataFrom = new DataFrom
				{
					NominatedFrom = fetchType,
					TopicName = userResponse.Topic?.Name
				}
			};
		}
		internal async Task<StoryRequest> GetStoryFromSuggestions()
		{
			const MetaDataType fetchType = MetaDataType.FetchUsersFollowSuggestions;
			var lookups = await GetLookupItems();
			var fetchUsers = (await _heartbeatLogic.GetMetaData<List<UserResponse<UserSuggestionDetails>>>(
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

			var user = fetchUsers?.ElementAtOrDefault(SecureRandom.Next(fetchUsers.Count - 1));

			if (user == null)
				return null;

			if (user.UserId == 0 || user.IsPrivate)
				return null;

			await AddObjectToLookup(user.UserId.ToString());
			return new StoryRequest
			{
				UserId = user.UserId,
				StoryId = null,
				ContainsItems = false,
				Items = null,
				User = new UserShort
				{
					Id = user.UserId,
					Username = user.Username,
					ProfilePicture = user.ProfilePicture
				},
				DataFrom = new DataFrom
				{
					NominatedFrom = fetchType,
					TopicName = user.Topic?.Name
				}
			};
		}
		internal async Task<StoryRequest> GetStoryFromFollowers()
		{
			const MetaDataType fetchType = MetaDataType.FetchUsersFollowerList;
			var lookups = await GetLookupItems();
			var fetchUsers = (await _heartbeatLogic.GetMetaData<List<UserResponse<string>>>(
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
			
			var user = fetchUsers?.ElementAtOrDefault(SecureRandom.Next(fetchUsers.Count - 1));

			if (user == null)
				return null;

			if (user.UserId == 0 || user.IsPrivate)
				return null;

			await AddObjectToLookup(user.UserId.ToString());

			return new StoryRequest
			{
				UserId = user.UserId,
				StoryId = null,
				ContainsItems = false,
				Items = null,
				User = new UserShort
				{
					Id = user.UserId,
					Username = user.Username,
					ProfilePicture = user.ProfilePicture
				},
				DataFrom = new DataFrom
				{
					NominatedFrom = fetchType,
					TopicName = user.Topic?.Name
				}
			};
		}
		internal async Task<StoryRequest> GetStoryFromLikers()
		{
			const MetaDataType fetchType = MetaDataType.FetchUsersViaPostLiked;
			var lookups = await GetLookupItems();
			var fetchUsers = (await _heartbeatLogic.GetMetaData<List<UserResponse<string>>>(
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

			var user = fetchUsers?.ElementAtOrDefault(SecureRandom.Next(fetchUsers.Count - 1));

			if (user == null)
				return null;

			if (user.UserId == 0 || user.IsPrivate)
				return null;

			await AddObjectToLookup(user.UserId.ToString());

			return new StoryRequest
			{
				UserId = user.UserId,
				StoryId = null,
				ContainsItems = false,
				Items = null,
				User = new UserShort
				{
					Id = user.UserId,
					Username = user.Username,
					ProfilePicture = user.ProfilePicture
				},
				DataFrom = new DataFrom
				{
					NominatedFrom = fetchType,
					TopicName = user.Topic?.Name
				}
			};
		}
		internal async Task<StoryRequest> GetStoryFromCommenters()
		{
			const MetaDataType fetchType = MetaDataType.FetchUsersViaPostCommented;
			var lookups = await GetLookupItems();
			var fetchUsers = (await _heartbeatLogic.GetMetaData<List<UserResponse<CommentResponse>>>(
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

			var user = fetchUsers?.ElementAtOrDefault(SecureRandom.Next(fetchUsers.Count - 1));

			if (user == null)
				return null;

			if (user.UserId == 0 || user.IsPrivate)
				return null;

			await AddObjectToLookup(user.UserId.ToString());

			return new StoryRequest
			{
				UserId = user.UserId,
				StoryId = null,
				ContainsItems = false,
				Items = null,
				User = new UserShort
				{
					Id = user.UserId,
					Username = user.Username,
					ProfilePicture = user.ProfilePicture
				},
				DataFrom = new DataFrom
				{
					NominatedFrom = fetchType,
					TopicName = user.Topic?.Name
				}
			};
		}
		internal async Task<StoryRequest> GetStoryFromFeed()
		{
			const MetaDataType fetchType = MetaDataType.FetchUsersStoryFeed;
			var lookups = await GetLookupItems();
			var fetchUsers = (await _heartbeatLogic.GetMetaData<List<UserResponse<InstaReelFeed>>>(
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

			var user = fetchUsers?.ElementAtOrDefault(SecureRandom.Next(fetchUsers.Count - 1));

			if (user == null)
				return null;

			if (user.UserId == 0 || user.IsPrivate)
				return null;

			await AddObjectToLookup(user.UserId.ToString());

			return new StoryRequest
			{
				UserId = user.UserId,
				StoryId = user.Object.Id,
				ContainsItems = user.Object.Items.Count > 0,
				Items = user.Object.Items.Count > 0 ? user.Object.Items
					.Select(_ => new StoryItem
					{
						StoryMediaId = _.Id,
						TakenAt = _.TakenAt
					}).ToList() : null,
				User = new UserShort
				{
					Id = user.UserId,
					Username = user.Username,
					ProfilePicture = user.ProfilePicture
				},
				DataFrom = new DataFrom
				{
					NominatedFrom = fetchType,
					TopicName = user.Topic?.Name
				}
			};
		}
		internal async Task<StoryRequest> GetStoryFromTopic()
		{
			const MetaDataType fetchType = MetaDataType.FetchUsersStoryViaTopics;
			var lookups = await GetLookupItems();
			var fetchUsers = (await _heartbeatLogic.GetMetaData<List<UserResponse<InstaStory>>>(
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

			var user = fetchUsers?.ElementAtOrDefault(SecureRandom.Next(fetchUsers.Count - 1));

			if (user == null)
				return null;

			if (user.UserId == 0 || user.IsPrivate)
				return null;

			await AddObjectToLookup(user.UserId.ToString());

			return new StoryRequest
			{
				UserId = user.UserId,
				StoryId = user.Object.Id,
				ContainsItems = user.Object.Items.Count > 0,
				Items = user.Object.Items.Count > 0 ? user.Object.Items
					.Select(_ => new StoryItem
					{
						StoryMediaId = _.Id,
						TakenAt = _.TakenAt
					}).ToList() : null,
				User = new UserShort
				{
					Id = user.UserId,
					Username = user.Username,
					ProfilePicture = user.ProfilePicture
				},
				DataFrom = new DataFrom
				{
					NominatedFrom = fetchType,
					TopicName = user.Topic?.Name
				}
			};
		}
		#endregion
		public StoryActionType GetStoryActionType(StoryActionType type)
		{
			var watchActionsChances = new List<Chance<StoryActionType>>();
			StoryActionType storyActionType;
			if (type == StoryActionType.Any)
			{
				var focusLocalMore = _user.Profile.AdditionalConfigurations.FocusLocalMore;
				if (_user.Profile.LocationTargetList != null && _user.Profile.LocationTargetList.Count > 0)
				{
					watchActionsChances.AddRange(new List<Chance<StoryActionType>>
					{
						new Chance<StoryActionType>
						{
							Object = StoryActionType.WatchFromFeed,
							Probability = 0.35
						},
						new Chance<StoryActionType>
						{
							Object = StoryActionType.WatchFromLocationTarget,
							Probability = focusLocalMore ? 0.25 : 0.10,
						},
						new Chance<StoryActionType>
						{
							Object = StoryActionType.WatchBasedOnSuggestedUsers,
							Probability = 0.05
						},
						new Chance<StoryActionType>
						{
							Object = StoryActionType.WatchByTopic,
							Probability = 0.15
						},
						new Chance<StoryActionType>
						{
							Object = StoryActionType.WatchBasedOnUserFollowers,
							Probability = focusLocalMore ? 0.10 : 0.25
						},
						new Chance<StoryActionType>
						{
							Object = StoryActionType.WatchFromComments,
							Probability = 0.10
						},
						new Chance<StoryActionType>
						{
							Object = StoryActionType.WatchFromLikes,
							Probability = 0.10
						}
					});
				}
				else
				{
					watchActionsChances.AddRange(new List<Chance<StoryActionType>>
					{
						new Chance<StoryActionType>
						{
							Object = StoryActionType.WatchFromFeed,
							Probability = 0.35
						},
						new Chance<StoryActionType>
						{
							Object = StoryActionType.WatchBasedOnSuggestedUsers,
							Probability = 0.05
						},
						new Chance<StoryActionType>
						{
							Object = StoryActionType.WatchByTopic,
							Probability = 0.15
						},
						new Chance<StoryActionType>
						{
							Object = StoryActionType.WatchBasedOnUserFollowers,
							Probability = 0.25
						},
						new Chance<StoryActionType>
						{
							Object = StoryActionType.WatchFromComments,
							Probability = 0.10
						},
						new Chance<StoryActionType>
						{
							Object = StoryActionType.WatchFromLikes,
							Probability = 0.10
						}
					});
				}

				storyActionType = SecureRandom.ProbabilityRoll(watchActionsChances);
			}
			else
			{
				storyActionType = type;
			}

			return storyActionType;
		}

	}
}