using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InstagramApiSharp.Classes.Models;
using Quarkless.Models.Actions;
using Quarkless.Models.Actions.Enums.ActionTypes;
using Quarkless.Models.Actions.Enums.StrategyType;
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
using Quarkless.Models.Stories;

namespace Quarkless.Logic.Actions.Action_Instances
{
	internal class WatchStoryAction : IActionCommit
	{
		private readonly IContentInfoBuilder _contentInfoBuilder;
		private readonly IHeartbeatLogic _heartbeatLogic;
		private readonly UserStoreDetails _user;
		private WatchStoryOptions _actionOptions;
		internal WatchStoryAction(UserStoreDetails userStoreDetails, IContentInfoBuilder contentInfoBuilder,
			IHeartbeatLogic heartbeatLogic)
		{
			_user = userStoreDetails;
			_contentInfoBuilder = contentInfoBuilder;
			_heartbeatLogic = heartbeatLogic;
			_actionOptions = new WatchStoryOptions();
		}

		#region Getter From heartbeat Logic
		private async Task<WatchStoryRequest> GetStoryFromLocationTarget()
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

			return new WatchStoryRequest
			{
				UserId = userResponse.User.UserId,
				StoryId = null,
				ContainsItems = false,
				Items = null
			};
		}
		private async Task<WatchStoryRequest> GetStoryFromSuggestions()
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

			return new WatchStoryRequest
			{
				UserId = user.UserId,
				StoryId = null,
				ContainsItems = false,
				Items = null
			};
		}
		private async Task<WatchStoryRequest> GetStoryFromFollowers()
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

			return new WatchStoryRequest
			{
				UserId = user.UserId,
				StoryId = null,
				ContainsItems = false,
				Items = null
			};
		}
		private async Task<WatchStoryRequest> GetStoryFromLikers()
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

			return new WatchStoryRequest
			{
				UserId = user.UserId,
				StoryId = null,
				ContainsItems = false,
				Items = null
			};
		}
		private async Task<WatchStoryRequest> GetStoryFromCommenters()
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

			return new WatchStoryRequest
			{
				UserId = user.UserId,
				StoryId = null,
				ContainsItems = false,
				Items = null
			};
		}
		private async Task<WatchStoryRequest> GetStoryFromFeed()
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

			return new WatchStoryRequest
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
		private async Task<WatchStoryRequest> GetStoryFromTopic()
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

			return new WatchStoryRequest
			{
				UserId = user.UserId,
				StoryId = user.Object.Id,
				ContainsItems = user.Object.Items.Count > 0,
				Items = user.Object.Items.Count > 0 ? user.Object.Items
					.Select(_=> new StoryItem
					{
						StoryMediaId = _.Id,
						TakenAt = _.TakenAt
					}).ToList() : null
			};
		}

		#endregion

		public async Task<ResultCarrier<EventActionModel>> PushAsync(DateTimeOffset executionTime)
		{
			Console.WriteLine($"Watch Story Action Started: {_user.AccountId}, {_user.InstagramAccountUsername}, {_user.InstagramAccountUser}");
			var results = new ResultCarrier<EventActionModel>();
			try
			{
				switch (_actionOptions.StrategySettings.StrategyType)
				{
					
					case WatchStoryStrategyType.Default: // watch a single users story
					{
						WatchStoryRequest watchStoryRequest = null;
						WatchStoryActionType watchStoryActionType;
						var watchActionsChances = new List<Chance<WatchStoryActionType>>();
						if (_actionOptions.WatchStoryActionType == WatchStoryActionType.Any)
						{
							var focusLocalMore = _user.Profile.AdditionalConfigurations.FocusLocalMore;
							if (_user.Profile.LocationTargetList != null && _user.Profile.LocationTargetList.Count > 0)
							{
								watchActionsChances.AddRange(new List<Chance<WatchStoryActionType>>
								{
									new Chance<WatchStoryActionType>
									{
										Object = WatchStoryActionType.WatchFromFeed,
										Probability = 0.35
									},
									new Chance<WatchStoryActionType>
									{
										Object = WatchStoryActionType.WatchFromLocationTarget,
										Probability = focusLocalMore ? 0.25 : 0.10,
									},
									new Chance<WatchStoryActionType>
									{
										Object = WatchStoryActionType.WatchBasedOnSuggestedUsers,
										Probability = 0.05
									},
									new Chance<WatchStoryActionType>
									{
										Object = WatchStoryActionType.WatchByTopic,
										Probability = 0.15
									},
									new Chance<WatchStoryActionType>
									{
										Object = WatchStoryActionType.WatchBasedOnUserFollowers,
										Probability = focusLocalMore ? 0.10 : 0.25
									},
									new Chance<WatchStoryActionType>
									{
										Object = WatchStoryActionType.WatchFromComments,
										Probability = 0.10
									},
									new Chance<WatchStoryActionType>
									{
										Object = WatchStoryActionType.WatchFromLikes,
										Probability = 0.10
									}
								});
							}
							else
							{
								watchActionsChances.AddRange(new List<Chance<WatchStoryActionType>>
								{
									new Chance<WatchStoryActionType>
									{
										Object = WatchStoryActionType.WatchFromFeed,
										Probability = 0.35
									},
									new Chance<WatchStoryActionType>
									{
										Object = WatchStoryActionType.WatchBasedOnSuggestedUsers,
										Probability = 0.05
									},
									new Chance<WatchStoryActionType>
									{
										Object = WatchStoryActionType.WatchByTopic,
										Probability = 0.15
									},
									new Chance<WatchStoryActionType>
									{
										Object = WatchStoryActionType.WatchBasedOnUserFollowers,
										Probability = 0.25
									},
									new Chance<WatchStoryActionType>
									{
										Object = WatchStoryActionType.WatchFromComments,
										Probability = 0.10
									},
									new Chance<WatchStoryActionType>
									{
										Object = WatchStoryActionType.WatchFromLikes,
										Probability = 0.10
									}
								});
							}

							watchStoryActionType = SecureRandom.ProbabilityRoll(watchActionsChances);
						}
						else
						{
							watchStoryActionType = _actionOptions.WatchStoryActionType;
						}

						switch (watchStoryActionType)
						{
							case WatchStoryActionType.WatchFromFeed:
								watchStoryRequest = await GetStoryFromFeed();
								break;
							case WatchStoryActionType.WatchByTopic:
								watchStoryRequest = await GetStoryFromTopic();
								break;
							case WatchStoryActionType.WatchBasedOnUserFollowers:
								watchStoryRequest = await GetStoryFromFollowers();
								break;
							case WatchStoryActionType.WatchFromLocationTarget:
								watchStoryRequest = await GetStoryFromLocationTarget();
								break;
							case WatchStoryActionType.WatchBasedOnSuggestedUsers:
								watchStoryRequest = await GetStoryFromSuggestions();
								break;
							case WatchStoryActionType.WatchFromLikes:
								watchStoryRequest = await GetStoryFromLikers();
								break;
							case WatchStoryActionType.WatchFromComments:
								watchStoryRequest = await GetStoryFromCommenters();
								break;
							default:
								throw new ArgumentOutOfRangeException();
						}

						if (watchStoryRequest == null)
						{
							results.IsSuccessful = false;
							results.Info = new ErrorResponse
							{
								Message = $"could not find any stories to watch, user: {_user.AccountId}, instaId: {_user.InstagramAccountUsername}",
								StatusCode = System.Net.HttpStatusCode.NotFound
							};
							return results;
						}

						var @event = new EventActionModel($"WatchStory_{_actionOptions.StrategySettings.StrategyType.ToString()}_{watchStoryActionType.ToString()}")
						{
							ActionType = ActionType.WatchStory,
							User = new UserStore
							{
								AccountId = _user.AccountId,
								InstagramAccountUsername = _user.InstagramAccountUsername,
								InstagramAccountUser = _user.InstagramAccountUser
							}
						};

						@event.DataObjects.Add(new EventBody(watchStoryRequest, watchStoryRequest.GetType(), executionTime));
						results.IsSuccessful = true;
						results.Results = @event;
						return results;
					}
					case WatchStoryStrategyType.MultipleUsers: // watch multiple users story
					{
						throw new NotImplementedException();
					}
					default:throw new ArgumentOutOfRangeException();
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
				Console.WriteLine($"Watch Story Action Ended: {_user.AccountId}, {_user.InstagramAccountUsername}, {_user.InstagramAccountUser}");
			}
		}

		public IActionCommit ModifyOptions(IActionOptions newOptions)
		{
			try
			{
				_actionOptions = newOptions as WatchStoryOptions;
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
			}
			return this;
		}
	}
}