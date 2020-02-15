using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InstagramApiSharp.Classes.Models;
using Quarkless.Models.Actions.Enums.ActionTypes;
using Quarkless.Models.Actions.Models;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Common.Extensions;
using Quarkless.Models.Common.Models;
using Quarkless.Models.Heartbeat;
using Quarkless.Models.Heartbeat.Interfaces;
using Quarkless.Models.InstagramSearch;
using Quarkless.Models.SearchResponse;
using Quarkless.Models.Stories;

namespace Quarkless.Logic.Actions.Action_Instances
{
	public class StoryActionFetcher
	{
		private readonly IHeartbeatLogic _heartbeatLogic;
		private readonly UserStoreDetails _user;

		public StoryActionFetcher(IHeartbeatLogic heartbeatLogic, UserStoreDetails user)
		{
			_heartbeatLogic = heartbeatLogic;
			_user = user;
		}

		#region Getter From heartbeat Logic
		internal async Task<StoryRequest> GetStoryFromLocationTarget()
		{
			var by = new By
			{
				ActionType = (int)ActionType.WatchStory,
				User = _user.Profile.InstagramAccountId
			};

			var fetchUsers = (await _heartbeatLogic.GetMetaData<Media>(
					new MetaDataFetchRequest
					{
						MetaDataType = MetaDataType.FetchMediaByUserLocationTargetList,
						ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
						InstagramId = _user.ShortInstagram.Id
					}))?.Where(exclude => !exclude.SeenBy.Any(e => e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Medias.Count > 0).ToList();

			var select = fetchUsers?.ElementAtOrDefault(SecureRandom.Next(fetchUsers.Count));
			if (select == null) return null;
			select.SeenBy.Add(by);

			await _heartbeatLogic.UpdateMetaData(new MetaDataCommitRequest<Media>
			{
				MetaDataType = MetaDataType.FetchMediaByUserLocationTargetList,
				ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
				InstagramId = _user.ShortInstagram.Id,
				Data = select
			});

			var userResponse = select.ObjectItem.Medias.FirstOrDefault();
			if (userResponse == null)
				return null;

			if (userResponse.User.UserId == 0 || userResponse.User.IsPrivate)
				return null;

			return new StoryRequest
			{
				UserId = userResponse.User.UserId,
				StoryId = null,
				ContainsItems = false,
				Items = null
			};
		}
		internal async Task<StoryRequest> GetStoryFromSuggestions()
		{
			var by = new By
			{
				ActionType = (int)ActionType.WatchStory,
				User = _user.Profile.InstagramAccountId
			};

			var fetchUsers = (await _heartbeatLogic.GetMetaData<List<UserResponse<UserSuggestionDetails>>>(
					new MetaDataFetchRequest
					{
						MetaDataType = MetaDataType.FetchUsersFollowSuggestions,
						ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
						InstagramId = _user.ShortInstagram.Id
					}))?.Where(exclude => !exclude.SeenBy.Any(e => e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Count > 0).ToList();

			var select = fetchUsers?.ElementAtOrDefault(SecureRandom.Next(fetchUsers.Count));

			if (select == null) return null;
			select.SeenBy.Add(by);

			await _heartbeatLogic.UpdateMetaData(new MetaDataCommitRequest<List<UserResponse<UserSuggestionDetails>>>
			{
				MetaDataType = MetaDataType.FetchUsersFollowSuggestions,
				ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
				InstagramId = _user.ShortInstagram.Id,
				Data = select
			});

			var user = select.ObjectItem.FirstOrDefault();
			if (user == null)
				return null;

			if (user.UserId == 0 || user.IsPrivate)
				return null;

			return new StoryRequest
			{
				UserId = user.UserId,
				StoryId = null,
				ContainsItems = false,
				Items = null
			};
		}
		internal async Task<StoryRequest> GetStoryFromFollowers()
		{
			var by = new By
			{
				ActionType = (int)ActionType.WatchStory,
				User = _user.Profile.InstagramAccountId
			};

			var fetchUsers = (await _heartbeatLogic.GetMetaData<List<UserResponse<string>>>(
					new MetaDataFetchRequest
					{
						MetaDataType = MetaDataType.FetchUsersFollowerList,
						ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
						InstagramId = _user.ShortInstagram.Id
					}))?.Where(exclude => !exclude.SeenBy.Any(e => e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Count > 0).ToList();

			var select = fetchUsers?.ElementAtOrDefault(SecureRandom.Next(fetchUsers.Count));

			if (select == null) return null;
			select.SeenBy.Add(by);

			await _heartbeatLogic.UpdateMetaData(new MetaDataCommitRequest<List<UserResponse<string>>>
			{
				MetaDataType = MetaDataType.FetchUsersFollowerList,
				ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
				InstagramId = _user.ShortInstagram.Id,
				Data = select
			});

			var user = select.ObjectItem.FirstOrDefault();
			if (user == null)
				return null;

			if (user.UserId == 0 || user.IsPrivate)
				return null;

			return new StoryRequest
			{
				UserId = user.UserId,
				StoryId = null,
				ContainsItems = false,
				Items = null
			};
		}
		internal async Task<StoryRequest> GetStoryFromLikers()
		{
			var by = new By
			{
				ActionType = (int)ActionType.WatchStory,
				User = _user.Profile.InstagramAccountId
			};

			var fetchUsers = (await _heartbeatLogic.GetMetaData<List<UserResponse<string>>>(
					new MetaDataFetchRequest
					{
						MetaDataType = MetaDataType.FetchUsersViaPostLiked,
						ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
						InstagramId = _user.ShortInstagram.Id
					}))?.Where(exclude => !exclude.SeenBy.Any(e => e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Count > 0).ToList();

			var select = fetchUsers?.ElementAtOrDefault(SecureRandom.Next(fetchUsers.Count));

			if (select == null) return null;
			select.SeenBy.Add(by);

			await _heartbeatLogic.UpdateMetaData(new MetaDataCommitRequest<List<UserResponse<string>>>
			{
				MetaDataType = MetaDataType.FetchUsersViaPostLiked,
				ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
				InstagramId = _user.ShortInstagram.Id,
				Data = select
			});

			var user = select.ObjectItem.FirstOrDefault();
			if (user == null)
				return null;

			if (user.UserId == 0 || user.IsPrivate)
				return null;

			return new StoryRequest
			{
				UserId = user.UserId,
				StoryId = null,
				ContainsItems = false,
				Items = null
			};
		}
		internal async Task<StoryRequest> GetStoryFromCommenters()
		{
			var by = new By
			{
				ActionType = (int)ActionType.WatchStory,
				User = _user.Profile.InstagramAccountId
			};

			var fetchUsers = (await _heartbeatLogic.GetMetaData<List<UserResponse<CommentResponse>>>(
					new MetaDataFetchRequest
					{
						MetaDataType = MetaDataType.FetchUsersViaPostCommented,
						ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
						InstagramId = _user.ShortInstagram.Id
					}))?.Where(exclude => !exclude.SeenBy.Any(e => e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Count > 0).ToList();

			var select = fetchUsers?.ElementAtOrDefault(SecureRandom.Next(fetchUsers.Count));

			if (select == null) return null;
			select.SeenBy.Add(by);

			await _heartbeatLogic.UpdateMetaData(new MetaDataCommitRequest<List<UserResponse<CommentResponse>>>
			{
				MetaDataType = MetaDataType.FetchUsersViaPostCommented,
				ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
				InstagramId = _user.ShortInstagram.Id,
				Data = select
			});

			var user = select.ObjectItem.FirstOrDefault();
			if (user == null)
				return null;

			if (user.UserId == 0 || user.IsPrivate)
				return null;

			return new StoryRequest
			{
				UserId = user.UserId,
				StoryId = null,
				ContainsItems = false,
				Items = null
			};
		}
		internal async Task<StoryRequest> GetStoryFromFeed()
		{
			var by = new By
			{
				ActionType = (int)ActionType.WatchStory,
				User = _user.Profile.InstagramAccountId
			};

			var fetchUsers = (await _heartbeatLogic.GetMetaData<List<UserResponse<InstaReelFeed>>>(
					new MetaDataFetchRequest
					{
						MetaDataType = MetaDataType.FetchUsersStoryFeed,
						ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
						InstagramId = _user.ShortInstagram.Id
					}))?.Where(exclude => !exclude.SeenBy.Any(e => e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Count > 0).ToList();

			var select = fetchUsers?.ElementAtOrDefault(SecureRandom.Next(fetchUsers.Count));

			if (select == null) return null;
			select.SeenBy.Add(by);

			await _heartbeatLogic.UpdateMetaData(new MetaDataCommitRequest<List<UserResponse<InstaReelFeed>>>
			{
				MetaDataType = MetaDataType.FetchUsersStoryFeed,
				ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
				InstagramId = _user.ShortInstagram.Id,
				Data = select
			});

			var user = select.ObjectItem.FirstOrDefault();
			if (user == null)
				return null;

			if (user.UserId == 0 || user.IsPrivate)
				return null;

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
					}).ToList() : null
			};
		}
		internal async Task<StoryRequest> GetStoryFromTopic()
		{
			var by = new By
			{
				ActionType = (int)ActionType.WatchStory,
				User = _user.Profile.InstagramAccountId
			};

			var fetchUsers = (await _heartbeatLogic.GetMetaData<List<UserResponse<InstaStory>>>(
					new MetaDataFetchRequest
					{
						MetaDataType = MetaDataType.FetchUsersStoryViaTopics,
						ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
						InstagramId = _user.ShortInstagram.Id
					}))?.Where(exclude => !exclude.SeenBy.Any(e => e.User == by.User && e.ActionType == by.ActionType))
				.Where(s => s.ObjectItem.Count > 0).ToList();

			var select = fetchUsers?.ElementAtOrDefault(SecureRandom.Next(fetchUsers.Count));

			if (select == null) return null;
			select.SeenBy.Add(by);

			await _heartbeatLogic.UpdateMetaData(new MetaDataCommitRequest<List<UserResponse<InstaStory>>>
			{
				MetaDataType = MetaDataType.FetchUsersStoryViaTopics,
				ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
				InstagramId = _user.ShortInstagram.Id,
				Data = select
			});

			var user = select.ObjectItem.FirstOrDefault();

			if (user == null)
				return null;

			if (user.UserId == 0 || user.IsPrivate)
				return null;

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
					}).ToList() : null
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