using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Quarkless.Base.Actions.Models;
using Quarkless.Base.Actions.Models.Enums.StrategyType;
using Quarkless.Base.Actions.Models.Factory.Action_Options;
using Quarkless.Base.Actions.Models.Interfaces;
using Quarkless.Base.ContentInfo.Models.Interfaces;
using Quarkless.Base.Heartbeat.Models;
using Quarkless.Base.Heartbeat.Models.Interfaces;
using Quarkless.Base.InstagramUser.Models;
using Quarkless.Common.Timeline.Models;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Common.Extensions;
using Quarkless.Models.Common.Models;
using Quarkless.Models.Common.Models.Carriers;
using Quarkless.Models.Common.Models.Resolver;
using Quarkless.Models.SearchResponse;

namespace Quarkless.Base.Actions.Logic.Action_Instances
{
	internal class UnFollowUserAction : IActionCommit
	{
		private readonly IContentInfoBuilder _contentInfoBuilder;
		private readonly IHeartbeatLogic _heartbeatLogic;
		private readonly UserStoreDetails _user;
		private UnfollowActionOptions _actionOptions;
		internal UnFollowUserAction(UserStoreDetails userStoreDetails, IContentInfoBuilder contentInfoBuilder,
			IHeartbeatLogic heartbeatLogic)
		{
			_user = userStoreDetails;
			_contentInfoBuilder = contentInfoBuilder;
			_heartbeatLogic = heartbeatLogic;

			_actionOptions = new UnfollowActionOptions();
		}

		public async Task<ResultCarrier<EventActionModel>> PushAsync(DateTimeOffset executionTime)
		{
			Console.WriteLine($"Unfollow Action Started: {_user.AccountId}, {_user.InstagramAccountUsername}, {_user.InstagramAccountUser}");
			var results = new ResultCarrier<EventActionModel>();
			try
			{
				const MetaDataType fetchType = MetaDataType.FetchUsersFollowingList;

				var fetchedUsers = (await _heartbeatLogic.GetMetaData<List<UserResponse<string>>>(
				new MetaDataFetchRequest
				{
					MetaDataType = fetchType,
					ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
					InstagramId = _user.ShortInstagram.Id,
					AccountId = _user.AccountId
				}))?.ToList();

				if (fetchedUsers == null || !fetchedUsers.Any())
				{
					results.IsSuccessful = false;
					results.Info = new ErrorResponse
					{
						Message = "No Users found to Unfollow"
					};
					return results;
				}
				switch (_actionOptions.StrategySettings.StrategyType)
				{
					case UnFollowStrategyType.Default:
					{
						const int maxAttempts = 3;
						var currentAttempt = 0;
						TryAgain:
						var nominatedUser = fetchedUsers?.ElementAt(SecureRandom.Next(fetchedUsers.Count - 1));

						var userToUnfollow = nominatedUser?.ObjectItem?.FirstOrDefault();
						if (userToUnfollow?.UserId == null)
						{
							if (currentAttempt > maxAttempts)
							{
								results.IsSuccessful = false;
								results.Info = new ErrorResponse
								{
									Message =
										"Tried 3 different users and failed to find a user to nominate for unfollow"
								};
								return results;
							}

							currentAttempt++;
							goto TryAgain;
						}
						var @event =
							new EventActionModel(
								$"UnfollowUser_{_actionOptions.StrategySettings.StrategyType.ToString()}")
							{
								ActionType = ActionType.UnFollowUser,
								User = new UserStore
								{
									AccountId = _user.AccountId,
									InstagramAccountUsername = _user.InstagramAccountUsername,
									InstagramAccountUser = _user.InstagramAccountUser
								}
							};

						var request = new FollowAndUnFollowUserRequest
						{
							UserId = userToUnfollow.UserId,
							User = new UserShort
							{
								Id = userToUnfollow.UserId,
								Username = userToUnfollow.Username,
								ProfilePicture = userToUnfollow.ProfilePicture
							},
							DataFrom = new DataFrom
							{
								NominatedFrom = fetchType,
								TopicName = userToUnfollow.Topic?.Name
							}
						};
						@event.DataObjects.Add(new EventBody(request, request.GetType(), executionTime));
						results.IsSuccessful = true;
						results.Results = @event;
						return results;
					}
					case UnFollowStrategyType.LeastEngagingN:
					{
						var @event = new EventActionModel(
						$"UnfollowUser_{_actionOptions.StrategySettings.StrategyType.ToString()}")
						{
							ActionType = ActionType.UnFollowUser,
							User = new UserStore
							{
								AccountId = _user.AccountId,
								InstagramAccountUsername = _user.InstagramAccountUsername,
								InstagramAccountUser = _user.InstagramAccountUser
							}
						};

						for (var i = 0; i < _actionOptions.StrategySettings.NumberOfUnfollows; i++)
						{
							var nominatedUser = fetchedUsers?.ElementAtOrDefault(i);
							var userToUnfollow = nominatedUser?.ObjectItem?.FirstOrDefault();

							if (userToUnfollow == null) continue;

							var request = new FollowAndUnFollowUserRequest
							{
								UserId = userToUnfollow.UserId,
								User = new UserShort
								{
									Id = userToUnfollow.UserId,
									Username = userToUnfollow.Username,
									ProfilePicture = userToUnfollow.ProfilePicture
								},
								DataFrom = new DataFrom
								{
									NominatedFrom = fetchType,
									TopicName = userToUnfollow.Topic?.Name
								}
							};
							@event.DataObjects.Add(new EventBody(request, request.GetType(), executionTime.AddSeconds(i * _actionOptions.StrategySettings.OffsetPerAction.TotalSeconds)));
						}

						results.IsSuccessful = true;
						results.Results = @event;
						return results;
					}
					default:throw new Exception("Invalid Strategy Type Selected");
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
				Console.WriteLine($"Unfollow Action Ended: {_user.AccountId}, {_user.InstagramAccountUsername}, {_user.InstagramAccountUser}");
			}
		}

		public IActionCommit ModifyOptions(IActionOptions newOptions)
		{
			try
			{
				_actionOptions = newOptions as UnfollowActionOptions;
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
			}
			return this;
		}

	}
}
