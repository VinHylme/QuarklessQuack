using System;
using System.Linq;
using System.Threading.Tasks;
using Quarkless.Analyser;
using Quarkless.Logic.Actions.AccountContainer;
using Quarkless.Logic.Actions.Limits;
using Quarkless.Logic.Agent;
using Quarkless.Models.Actions.Enums.StrategyType;
using Quarkless.Models.Actions.Factory.Action_Options;
using Quarkless.Models.Actions.Factory.Action_Options.StrategySettings;
using Quarkless.Models.Actions.Interfaces;
using Quarkless.Models.Agent.Interfaces;
using Quarkless.Models.Auth.Enums;
using Quarkless.Models.Auth.Interfaces;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Common.Extensions;
using Quarkless.Models.Common.Models;
using Quarkless.Models.Common.Objects;
using Quarkless.Models.InstagramAccounts;
using Quarkless.Models.InstagramAccounts.Enums;
using Quarkless.Models.InstagramAccounts.Interfaces;
using Quarkless.Models.Library.Interfaces;
using Quarkless.Models.Profile.Interfaces;
using Quarkless.Models.ResponseResolver.Interfaces;
using Quarkless.Models.Services.Automation.Interfaces;
using Quarkless.Models.Shared.Extensions;
using Quarkless.Models.Storage.Interfaces;
using Quarkless.Models.Timeline;
using Quarkless.Models.Timeline.Enums;
using Quarkless.Models.Timeline.Interfaces;
using Quarkless.Models.Timeline.TaskScheduler;
using UserStoreDetails = Quarkless.Models.Actions.Models.UserStoreDetails;
namespace Quarkless.Logic.Services.Automation
{
	public sealed class AgentManager : IAgentManager
	{
		#region Init
		private const double UNFOLLOW_AMOUNT = 0.25;
		private readonly XRange _unfollowPurgeCycle = new XRange(42,52);
		
		private readonly IInstagramAccountLogic _instagramAccountLogic;
		private readonly IProfileLogic _profileLogic;
		private readonly ITimelineLogic _timelineLogic;
		private readonly ITimelineEventLogLogic _timelineEventLogs;
		private readonly ILibraryLogic _libraryLogic;
		private readonly IS3BucketLogic _s3BucketLogic;
		private readonly IPostAnalyser _postAnalyser;
		private readonly IAgentLogic _agentLogic;
		private readonly IActionCommitFactory _actionCommitFactory;
		private readonly IAccountRepository _accountRepository;
		private readonly IResponseResolver _responseResolver;
		public AgentManager(IInstagramAccountLogic instagramAccountLogic, IProfileLogic profileLogic,
			ITimelineLogic timelineLogic, ITimelineEventLogLogic eventLogLogic, ILibraryLogic libraryLogic,
			IS3BucketLogic s3Bucket, IPostAnalyser postAnalyser, IActionCommitFactory actionCommitFactory,
			IAccountRepository accountRepository, IResponseResolver responseResolver)
		{
			_timelineLogic = timelineLogic;
			_timelineEventLogs = eventLogLogic;
			_instagramAccountLogic = instagramAccountLogic;
			_profileLogic = profileLogic;
			_libraryLogic = libraryLogic;
			_s3BucketLogic = s3Bucket;
			_postAnalyser = postAnalyser;
			_actionCommitFactory = actionCommitFactory;
			_accountRepository = accountRepository;
			_agentLogic = new AgentLogic(instagramAccountLogic);
			_responseResolver = responseResolver;
		}
		#endregion

		private async Task<AddEventResponse> AddToTimeline(EventActionOptions @event, Limits limits)
		{
			if (@event == null || limits == null) return null;
			try
			{
				var completedActionsDaily = await SuccessfulActionCount((ActionType)@event.ActionType,
					@event.User.AccountId,
					@event.User.InstagramAccountUser);

				var completedActionsHourly = await SuccessfulActionCount((ActionType)@event.ActionType, 
					@event.User.AccountId,
					@event.User.InstagramAccountUser, isHourly:true);

				switch ((ActionType)@event.ActionType)
				{
					case ActionType.WatchStory when completedActionsDaily >= limits.DailyLimits.WatchStoryLimit:
						return new AddEventResponse
						{
							Event = @event,
							ContainsErrors = false,
							HasCompleted = false,
							DailyLimitReached = true
						};
					case ActionType.WatchStory when completedActionsHourly >= limits.HourlyLimits.WatchStoryLimit:
						return new AddEventResponse
						{
							Event = @event,
							ContainsErrors = false,
							HasCompleted = false,
							HourlyLimitReached = true
						};
					case ActionType.CreatePost when completedActionsDaily >= limits.DailyLimits.CreatePostLimit:
						return new AddEventResponse
						{
							Event = @event,
							ContainsErrors = false,
							HasCompleted = false,
							DailyLimitReached = true
						};
					case ActionType.CreatePost when completedActionsHourly >= limits.HourlyLimits.CreatePostLimit:
						return new AddEventResponse
						{
							Event = @event,
							ContainsErrors = false,
							HasCompleted = false,
							HourlyLimitReached = true
						};
					case ActionType.SendDirectMessage when completedActionsDaily >= limits.DailyLimits.SendMessageLimit:
						return new AddEventResponse
						{
							Event = @event,
							ContainsErrors = false,
							HasCompleted = false,
							DailyLimitReached = true
						};
					case ActionType.SendDirectMessage when completedActionsHourly >= limits.HourlyLimits.SendMessageLimit:
						return new AddEventResponse
						{
							Event = @event,
							ContainsErrors = false,
							HasCompleted = false,
							HourlyLimitReached = true
						};
					case ActionType.CreateCommentMedia when completedActionsDaily >= limits.DailyLimits.CreateCommentLimit:
						return new AddEventResponse
						{
							Event = @event,
							ContainsErrors = false,
							HasCompleted = false,
							DailyLimitReached = true
						};
					case ActionType.CreateCommentMedia when completedActionsHourly >= limits.HourlyLimits.CreateCommentLimit:
						return new AddEventResponse
						{
							Event = @event,
							ContainsErrors = false,
							HasCompleted = false,
							HourlyLimitReached = true
						};
					case ActionType.LikePost when completedActionsDaily >= limits.DailyLimits.LikePostLimit:
						return new AddEventResponse
						{
							Event = @event,
							ContainsErrors = false,
							HasCompleted = false,
							DailyLimitReached = true
						};
					case ActionType.LikePost when completedActionsHourly >= limits.HourlyLimits.LikePostLimit:
						return new AddEventResponse
						{
							Event = @event,
							ContainsErrors = false,
							HasCompleted = false,
							HourlyLimitReached = true
						};
					case ActionType.LikeComment when completedActionsDaily >= limits.DailyLimits.LikeCommentLimit:
						return new AddEventResponse
						{
							Event = @event,
							ContainsErrors = false,
							HasCompleted = false,
							DailyLimitReached = true
						};
					case ActionType.LikeComment when completedActionsHourly >= limits.HourlyLimits.LikeCommentLimit:
						return new AddEventResponse
						{
							Event = @event,
							ContainsErrors = false,
							HasCompleted = false,
							HourlyLimitReached = true
						};
					case ActionType.FollowUser when completedActionsDaily >= limits.DailyLimits.FollowPeopleLimit:
						return new AddEventResponse
						{
							Event = @event,
							ContainsErrors = false,
							HasCompleted = false,
							DailyLimitReached = true
						};
					case ActionType.FollowUser when completedActionsHourly >= limits.HourlyLimits.FollowPeopleLimit:
						return new AddEventResponse
						{
							Event = @event,
							ContainsErrors = false,
							HasCompleted = false,
							HourlyLimitReached = true
						};
				}

				_timelineLogic.AddEventToTimeline(@event);
				return new AddEventResponse
				{
					HasCompleted = true,
					Event = @event
				};
			}
			catch (Exception ee)
			{
				return new AddEventResponse
				{
					ContainsErrors = true,
					Event = @event,
					Errors = new TimelineErrorResponse
					{
						Exception = ee,
						Message = ee.Message
					}
				};
			}
		}

		private async Task<int> SuccessfulActionCount(ActionType actionName, string accountId, string instagramAccountId, 
		int limit = 6000, bool isHourly = false)
		{
			
			var logs = await _timelineEventLogs.GetLogsForUser(accountId, instagramAccountId, limit);
			var startDate = DateTime.UtcNow;
			var endDate = !isHourly ? startDate.AddDays(-1).AddTicks(1) : startDate.AddHours(-1).AddTicks(1);

			return logs.Count(x => x.Status == TimelineEventStatus.Success
				&& (x.DateAdded <= startDate && x.DateAdded >= endDate) &&  x.ActionType == actionName
			);
		}

		private int ScheduledCount(string accountId, string instagramId)
		{
			var scheduled = _timelineLogic.GetScheduledEventsForUser(accountId, instagramId,150);
			return scheduled?.Count() ?? -1;
		}

		// TODO: MAKE SURE ALL USERS ARE BUSINESS ACCOUNTS AND USERS OVER 100 FOLLOWERS
		// TODO: BASE THEIR POSTING ON WHICH HOUR WAS MOST POPULAR
		public async Task Start(string accountId, string instagramAccountId)
		{
			Console.WriteLine($"Started Agent Automation for {accountId}/{instagramAccountId}");
			while (!ShutDownInstance.IsShutDown)
			{
				try
				{
					#region Initialise Account Details
					var account = await _agentLogic.GetAccount(accountId, instagramAccountId);
					if (account == null) return;

					await _responseResolver.CheckBlockStates(account);

					var accAuthDetails = await _accountRepository.GetAccountByUsername(accountId);
					if (accAuthDetails == null) return;
					
					var profile = await _profileLogic.GetProfile(account.AccountId, account.Id);
					if (profile == null) return;

					account.UserLimits = ActionLimits.SetLimits(accAuthDetails.Roles.FirstOrDefault()
						.GetValueFromDescription<AuthTypes>(), account.DateAdded.Value);

					await _instagramAccountLogic.PartialUpdateInstagramAccount(account.AccountId, account.Id,
						new InstagramAccountModel {UserLimits = account.UserLimits});
					
					var userStoreDetails = new UserStoreDetails
					{
						AccountId = account.AccountId,
						InstagramAccountUsername = account.Username,
						InstagramAccountUser = account.Id,
						Profile = profile,
						ShortInstagram = account,
						MessagesTemplates = await _libraryLogic.GetSavedMessagesForUser(account.Id)
					};

					var nextAvailableDate =
						_timelineLogic.PickAGoodTime(userStoreDetails.AccountId, userStoreDetails.InstagramAccountUser);

					var availableDate = nextAvailableDate ?? DateTime.UtcNow;

					var scheduledForUser =
						ScheduledCount(userStoreDetails.AccountId, userStoreDetails.InstagramAccountUser);
					#endregion
					
					#region Unfollow Purge Cycle

					if (account.LastPurgeCycle == null)
					{
						await _instagramAccountLogic.PartialUpdateInstagramAccount(userStoreDetails.AccountId,
							userStoreDetails.InstagramAccountUser,
							new InstagramAccountModel
							{
								LastPurgeCycle = DateTime.UtcNow
									.AddHours(SecureRandom.Next(_unfollowPurgeCycle.Min, _unfollowPurgeCycle.Max))
							});
					}
					else
					{
						if (DateTime.UtcNow > account.LastPurgeCycle)
						{
							if (account.FollowingCount != null)
							{
								var response = await _actionCommitFactory
									.Create(ActionType.UnFollowUser, userStoreDetails)
									.ModifyOptions(new UnfollowActionOptions(new UnFollowStrategySettings(
										UnFollowStrategyType.LeastEngagingN,
										TimeSpan.FromSeconds(SecureRandom.Next(
											UnfollowActionOptions.TimeFrameSeconds.Min,
											UnfollowActionOptions.TimeFrameSeconds.Max)),
										(int) (account.FollowingCount.Value * UNFOLLOW_AMOUNT))))
									.PushAsync(availableDate.AddSeconds(UnfollowActionOptions.TimeFrameSeconds.Max));

								if (response.IsSuccessful)
								{
									foreach (var resultData in response.Results.DataObjects)
									{
										_timelineLogic.AddEventToTimeline(new EventActionOptions
										{
											ActionType = (int) ActionType.UnFollowUser,
											ActionDescription = response.Results.ActionDescription,
											DataObject = new EventActionOptions.EventBody
											{
												Body = resultData.Body,
												BodyType = resultData.BodyType
											},
											ExecutionTime = resultData.ExecutionTime,
											User = new UserStore
											{
												AccountId = userStoreDetails.AccountId,
												InstagramAccountUsername = userStoreDetails.InstagramAccountUsername,
												InstagramAccountUser = userStoreDetails.InstagramAccountUser
											}
										});
									}

									await _instagramAccountLogic.PartialUpdateInstagramAccount(
										userStoreDetails.AccountId,
										userStoreDetails.InstagramAccountUser,
										new InstagramAccountModel
										{
											LastPurgeCycle = DateTime.UtcNow
												.AddHours(SecureRandom.Next(_unfollowPurgeCycle.Min,
													_unfollowPurgeCycle.Max))
										});
								}
							}
						}
					}

					#endregion

					#region Initialise Actions

					const double likePostChance = 0.25;
					const double likeCommentChance = 0.20;
					const double followUserChance = 0.20;
					const double createCommentChance = 0.10;
					const double createPostChance = 0.05;
					const double watchStoryChance = 0.20;

					var actionsContainerManager = new ActionsContainerManager();

					if (account.BlockedActions.All(_ => _.ActionType != ActionType.LikePost))
					{
						actionsContainerManager.RegisterAction(_actionCommitFactory
								.Create(ActionType.LikePost, userStoreDetails),
							availableDate.AddMinutes(SecureRandom.Next(1, 4)), likePostChance);
					}

					if (account.BlockedActions.All(_ => _.ActionType != ActionType.LikeComment))
					{
						actionsContainerManager.RegisterAction(
							_actionCommitFactory.Create(ActionType.LikeComment, userStoreDetails),
							availableDate.AddMinutes(SecureRandom.Next(4)), likeCommentChance);
					}

					if (account.BlockedActions.All(_ => _.ActionType != ActionType.FollowUser))
					{
						actionsContainerManager.RegisterAction(
							_actionCommitFactory.Create(ActionType.FollowUser, userStoreDetails),
							availableDate.AddMinutes(SecureRandom.Next(1, 4)), followUserChance);
					}

					if (account.BlockedActions.All(_ => _.ActionType != ActionType.CreateCommentMedia))
					{
						actionsContainerManager.RegisterAction(
							_actionCommitFactory.Create(ActionType.CreateCommentMedia, userStoreDetails),
							availableDate.AddMinutes(SecureRandom.Next(1, 4)), createCommentChance);
					}

					if (account.BlockedActions.All(_ => _.ActionType != ActionType.CreatePost))
					{
						actionsContainerManager.RegisterAction(
							_actionCommitFactory.Create(ActionType.CreatePost, userStoreDetails)
								.ModifyOptions(new PostActionOptions(_postAnalyser, _s3BucketLogic)),
							availableDate.AddMinutes(SecureRandom.Next(1, 5)), createPostChance);
					}

					if (account.BlockedActions.All(_ => _.ActionType != ActionType.WatchStory))
					{
						actionsContainerManager.RegisterAction(_actionCommitFactory.Create(ActionType.WatchStory,
								userStoreDetails), availableDate.AddMinutes(SecureRandom.Next(1, 2)), watchStoryChance);
					}

					#endregion

					#region Agent State Handler
					
					if (account.AgentState == (int) AgentState.NotStarted)
					{
						account.AgentState = (int) AgentState.Running;
						await _instagramAccountLogic.PartialUpdateInstagramAccount(userStoreDetails.AccountId,
							userStoreDetails.InstagramAccountUser, new InstagramAccountModel
							{
								AgentState = account.AgentState,
							});
					}

					if (actionsContainerManager.RegisteredActions.Count <= 0)
					{
						await _instagramAccountLogic.PartialUpdateInstagramAccount(userStoreDetails.AccountId, 
							userStoreDetails.InstagramAccountUser, new InstagramAccountModel
						{
							AgentState = (int) AgentState.Blocked
						});
					}

					switch (account.AgentState)
					{
						case (int) AgentState.Running:
						{
							if (scheduledForUser != -1)
							{
								if (scheduledForUser > 80)
								{
									account.AgentState = (int) AgentState.Sleeping;

									await _instagramAccountLogic.PartialUpdateInstagramAccount(
										userStoreDetails.AccountId,
										userStoreDetails.InstagramAccountUser, new InstagramAccountModel {
											AgentState = account.AgentState,
										});
								}
							}

							var nominatedAction = actionsContainerManager.GetRandomAction();
							actionsContainerManager.AddWork(nominatedAction);
							await actionsContainerManager.RunAction();

							var actionFinished = actionsContainerManager.GetFinishedAction();

							if (actionFinished == null)
								break;

							foreach (var eventBody in actionFinished.DataObjects)
							{
								var timeSett = actionsContainerManager.FindActionLimit(actionFinished.ActionType);

								nextAvailableDate = _timelineLogic.PickAGoodTime(userStoreDetails.AccountId,
									userStoreDetails.InstagramAccountUser, actionFinished.ActionType);

								actionsContainerManager.HasMetTimeLimit();

								var getDate = nextAvailableDate ?? DateTime.UtcNow;

								eventBody.ExecutionTime = getDate.AddSeconds(timeSett.Max);

								var res = await AddToTimeline(new EventActionOptions
								{
									ActionType = (int) actionFinished.ActionType,
									ActionDescription = actionFinished.ActionDescription,
									DataObject = new EventActionOptions.EventBody
									{
										Body = eventBody.Body,
										BodyType = eventBody.BodyType
									},
									ExecutionTime = eventBody.ExecutionTime,
									User = new UserStore
									{
										InstagramAccountUsername = actionFinished.User.InstagramAccountUsername,
										AccountId = actionFinished.User.AccountId,
										InstagramAccountUser = actionFinished.User.InstagramAccountUser
									}
								}, account.UserLimits);

								if (res.DailyLimitReached)
									await _instagramAccountLogic.AddBlockedAction(account.Id, actionFinished.ActionType, DateTime.UtcNow.AddHours(12));
								
								else if (res.HourlyLimitReached)
									await _instagramAccountLogic.AddBlockedAction(account.Id, actionFinished.ActionType, DateTime.Now.AddHours(1));
							}

							break;
						}
						case (int) AgentState.Sleeping when scheduledForUser == -1:
							account.SleepTimeRemaining = DateTime.UtcNow
								.AddMinutes(SecureRandom.Next(60, 350) + (SecureRandom.NextDouble() * 2));

							account.AgentState = (int) AgentState.DeepSleep;

							await _instagramAccountLogic.PartialUpdateInstagramAccount(userStoreDetails.AccountId,
								userStoreDetails.InstagramAccountUser, new InstagramAccountModel
								{
									SleepTimeRemaining = account.SleepTimeRemaining,
									AgentState = account.AgentState
								});

							break;
						case (int) AgentState.Sleeping:
						{
							if (scheduledForUser <= 10)
							{
								account.SleepTimeRemaining = DateTime.UtcNow
									.AddMinutes(SecureRandom.Next(60, 350) + (SecureRandom.NextDouble() * 2));

								account.AgentState = (int) AgentState.DeepSleep;

								await _instagramAccountLogic.PartialUpdateInstagramAccount(userStoreDetails.AccountId,
									userStoreDetails.InstagramAccountUser, new InstagramAccountModel
									{
										SleepTimeRemaining = account.SleepTimeRemaining,
										AgentState = account.AgentState
									});
							}

							break;
						}
						case (int) AgentState.Blocked:
						{
							var items = _timelineLogic.GetScheduledEventsForUser(userStoreDetails.AccountId,
								userStoreDetails.InstagramAccountUser, 2000).ToList();
							items.ForEach(_ => { _timelineLogic.DeleteEvent(_.ItemId); });
							break;
						}
						case (int) AgentState.AwaitingActionFromUser:
						{
							var items = _timelineLogic.GetScheduledEventsForUser(userStoreDetails.AccountId,
								userStoreDetails.InstagramAccountUser, 2000).ToList();
							items.ForEach(_ => { _timelineLogic.DeleteEvent(_.ItemId); });
							break;
						}
						case (int) AgentState.DeepSleep when account.SleepTimeRemaining.HasValue:
						{
							if (DateTime.UtcNow > account.SleepTimeRemaining.Value)
							{
								account.AgentState = (int) AgentState.Running;
								await _instagramAccountLogic.PartialUpdateInstagramAccount(userStoreDetails.AccountId,
									userStoreDetails.InstagramAccountUser, new InstagramAccountModel
									{
										AgentState = account.AgentState,
									});
							}

							break;
						}
						case (int) AgentState.DeepSleep:
							account.AgentState = (int) AgentState.Running;
							await _instagramAccountLogic.PartialUpdateInstagramAccount(userStoreDetails.AccountId,
								userStoreDetails.InstagramAccountUser, new InstagramAccountModel
								{
									AgentState = account.AgentState,
								});
							break;
						case (int) AgentState.Stopped:
							break;
					}

					#endregion
				}
				catch (Exception err)
				{
					Console.WriteLine(err);
				}
				await Task.Delay(TimeSpan.FromSeconds(SecureRandom.Next(1,6)));
			}
			Console.WriteLine($"Ended Agent Automation for {accountId}/{instagramAccountId}");
		}
	}
}
