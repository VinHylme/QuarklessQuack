using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Quarkless.Models.Actions;
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
			Console.WriteLine($"Unfollow Action Started: {_user.OAccountId}, {_user.OInstagramAccountUsername}, {_user.OInstagramAccountUser}");
			var results = new ResultCarrier<EventActionModel>();
			try
			{
				var by = new By
				{
					ActionType = (int)ActionType.UnFollowUser,
					User = _user.Profile.InstagramAccountId
				};

				var fetchedUsers = (await _heartbeatLogic.GetMetaData<List<UserResponse<string>>>(new MetaDataFetchRequest
				{
					MetaDataType = MetaDataType.FetchUsersFollowingList,
					ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
					InstagramId = _user.ShortInstagram.Id
				}))?.Where(exclude => !exclude.SeenBy.Any(e => e.User == by.User && e.ActionType == by.ActionType))
					.ToList();
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
						var nominatedUser = fetchedUsers?.ElementAt(SecureRandom.Next(fetchedUsers.Count));
						var userId = nominatedUser?.ObjectItem?.FirstOrDefault()?.UserId;
						if (userId == null)
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

						nominatedUser.SeenBy.Add(@by);

						await _heartbeatLogic.UpdateMetaData(new MetaDataCommitRequest<List<UserResponse<string>>>
						{
							MetaDataType = MetaDataType.FetchUsersFollowingList,
							ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
							InstagramId = _user.ShortInstagram.Id,
							Data = nominatedUser
						});
						var @event =
							new EventActionModel(
								$"UnfollowUser_{_actionOptions.StrategySettings.StrategyType.ToString()}")
							{
								ActionType = ActionType.UnFollowUser,
								User = new UserStore
								{
									OAccountId = _user.OAccountId,
									OInstagramAccountUsername = _user.OInstagramAccountUsername,
									OInstagramAccountUser = _user.OInstagramAccountUser
								}
							};

						@event.DataObjects.Add(new EventBody(userId, userId.GetType(), executionTime));
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
								OAccountId = _user.OAccountId,
								OInstagramAccountUsername = _user.OInstagramAccountUsername,
								OInstagramAccountUser = _user.OInstagramAccountUser
							}
						};

						for (var i = 0; i < _actionOptions.StrategySettings.NumberOfUnfollows; i++)
						{
							var nominatedUser = fetchedUsers?.ElementAtOrDefault(i);
							var userId = nominatedUser?.ObjectItem?.FirstOrDefault()?.UserId;
							if (userId == null) continue;

							nominatedUser.SeenBy.Add(@by);

							await _heartbeatLogic.UpdateMetaData(new MetaDataCommitRequest<List<UserResponse<string>>>
							{
								MetaDataType = MetaDataType.FetchUsersFollowingList,
								ProfileCategoryTopicId = _user.Profile.ProfileTopic.Category._id,
								InstagramId = _user.ShortInstagram.Id,
								Data = nominatedUser
							});

							@event.DataObjects.Add(new EventBody(userId.Value, userId.Value.GetType(), executionTime.AddSeconds(i * _actionOptions.StrategySettings.OffsetPerAction.TotalSeconds)));
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
				Console.WriteLine($"Unfollow Action Ended: {_user.OAccountId}, {_user.OInstagramAccountUsername}, {_user.OInstagramAccountUser}");
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
