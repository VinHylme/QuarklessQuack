using System;
using System.Linq;
using System.Threading.Tasks;
using Quarkless.Analyser;
using Quarkless.Logic.Actions.AccountContainer;
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
using Quarkless.Models.Services.Automation.Interfaces;
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
		private readonly XRange _unfollowPurgeCycle = new XRange(16,33);
		
		private readonly IInstagramAccountLogic _instagramAccountLogic;
		private readonly IProfileLogic _profileLogic;
		private readonly ITimelineLogic _timelineLogic;
		private readonly ITimelineEventLogLogic _timelineEventLogs;
		private readonly IAuthHandler _authHandler;
		private readonly ILibraryLogic _libraryLogic;
		private readonly IS3BucketLogic _s3BucketLogic;
		private readonly IPostAnalyser _postAnalyser;
		private readonly IAgentLogic _agentLogic;
		private readonly IActionCommitFactory _actionCommitFactory;

		public AgentManager(IInstagramAccountLogic instagramAccountLogic, IProfileLogic profileLogic,
			ITimelineLogic timelineLogic, ITimelineEventLogLogic eventLogLogic,IAuthHandler authHandler, 
			ILibraryLogic libraryLogic, IS3BucketLogic s3Bucket, IPostAnalyser postAnalyser, 
			IActionCommitFactory actionCommitFactory)
		{
			_timelineLogic = timelineLogic;
			_timelineEventLogs = eventLogLogic;
			_instagramAccountLogic = instagramAccountLogic;
			_profileLogic = profileLogic;
			_authHandler = authHandler;
			_libraryLogic = libraryLogic;
			_s3BucketLogic = s3Bucket;
			_postAnalyser = postAnalyser;
			_actionCommitFactory = actionCommitFactory;
			_agentLogic = new AgentLogic(instagramAccountLogic);
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
							DailyLimitReached = true
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
							DailyLimitReached = true
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

		private Limits SetLimits(AuthTypes authType, bool isWarmingUp = false)
		{
			if (isWarmingUp)
			{
				return new Limits
				{
					DailyLimits = new DailyActions
					{
						SendMessageLimit = 1,
						CreateCommentLimit = 4,
						CreatePostLimit = 2,
						FollowPeopleLimit = 12,
						FollowTopicLimit = 12,
						LikeCommentLimit = 20,
						LikePostLimit = 20
					},
					HourlyLimits = new HourlyActions
					{
						SendMessageLimit = 1,
						CreatePostLimit = 1,
						CreateCommentLimit = 1,
						FollowPeopleLimit = 2,
						FollowTopicLimit = 2,
						LikeCommentLimit = 4,
						LikePostLimit = 4
					}
				};
			}
			switch (authType)
			{
				case AuthTypes.Admin:
					return new Limits
					{
						DailyLimits = new DailyActions
						{
							SendMessageLimit = 200,
							CreateCommentLimit = 500,
							CreatePostLimit = 24,
							FollowPeopleLimit = 225,
							FollowTopicLimit = 225,
							LikeCommentLimit = 900,
							LikePostLimit = 900
						},
						HourlyLimits = new HourlyActions
						{
							SendMessageLimit = 12,
							CreatePostLimit = 5,
							CreateCommentLimit = 55,
							FollowPeopleLimit = 55,
							FollowTopicLimit = 55,
							LikeCommentLimit = 55,
							LikePostLimit = 55
						}
					};
				case AuthTypes.BasicUsers:
					return new Limits
					{
						DailyLimits = new DailyActions
						{
							SendMessageLimit = 24,
							CreateCommentLimit = 100,
							CreatePostLimit = 8,
							FollowPeopleLimit = 80,
							FollowTopicLimit = 80,
							LikeCommentLimit = 200,
							LikePostLimit = 200
						},
						HourlyLimits = new HourlyActions
						{
							SendMessageLimit = 3,
							CreatePostLimit = 2,
							CreateCommentLimit = 30,
							FollowPeopleLimit = 30,
							FollowTopicLimit = 30,
							LikeCommentLimit = 30,
							LikePostLimit = 30
						}
					};
				case AuthTypes.EnterpriseUsers:
					return new Limits
					{
						DailyLimits = new DailyActions
						{
							SendMessageLimit = 185,
							CreateCommentLimit = 499,
							CreatePostLimit = 23,
							FollowPeopleLimit = 220,
							FollowTopicLimit = 220,
							LikeCommentLimit = 899,
							LikePostLimit = 899
						},
						HourlyLimits = new HourlyActions
						{
							SendMessageLimit = 10,
							CreatePostLimit = 4,
							CreateCommentLimit = 53,
							FollowPeopleLimit = 53,
							FollowTopicLimit = 53,
							LikeCommentLimit = 53,
							LikePostLimit = 53
						}
					};
				case AuthTypes.Expired:
					return new Limits
					{
						DailyLimits = new DailyActions
						{
							SendMessageLimit = 0,
							CreateCommentLimit = 0,
							CreatePostLimit = 0,
							FollowPeopleLimit = 0,
							FollowTopicLimit = 0,
							LikeCommentLimit = 0,
							LikePostLimit = 0
						},
						HourlyLimits = new HourlyActions
						{
							SendMessageLimit = 0,
							CreatePostLimit = 0,
							CreateCommentLimit = 0,
							FollowPeopleLimit = 0,
							FollowTopicLimit = 0,
							LikeCommentLimit = 0,
							LikePostLimit = 0
						}
					};
				case AuthTypes.PremiumUsers:
					return new Limits
					{
						DailyLimits = new DailyActions
						{
							SendMessageLimit = 125,
							CreateCommentLimit = 280,
							CreatePostLimit = 15,
							FollowPeopleLimit = 125,
							FollowTopicLimit = 125,
							LikeCommentLimit = 600,
							LikePostLimit = 600
						},
						HourlyLimits = new HourlyActions
						{
							SendMessageLimit = 7,
							CreatePostLimit = 3,
							CreateCommentLimit = 42,
							FollowPeopleLimit = 42,
							FollowTopicLimit = 42,
							LikeCommentLimit = 42,
							LikePostLimit = 42
						}
					};
				case AuthTypes.TrialUsers:
					return new Limits
					{
						DailyLimits = new DailyActions
						{
							SendMessageLimit = 10,
							CreateCommentLimit = 60,
							CreatePostLimit = 4,
							FollowPeopleLimit = 40,
							FollowTopicLimit = 40,
							LikeCommentLimit = 100,
							LikePostLimit = 100
						},
						HourlyLimits = new HourlyActions
						{
							SendMessageLimit = 1,
							CreatePostLimit = 1,
							CreateCommentLimit = 15,
							FollowPeopleLimit = 15,
							FollowTopicLimit = 15,
							LikeCommentLimit = 15,
							LikePostLimit = 15
						}
					};
				default:
					return new Limits
					{
						DailyLimits = new DailyActions
						{
							CreateCommentLimit = 0,
							CreatePostLimit = 0,
							FollowPeopleLimit = 0,
							FollowTopicLimit = 0,
							LikeCommentLimit = 0,
							LikePostLimit = 0
						},
						HourlyLimits = new HourlyActions
						{
							CreatePostLimit = 0,
							CreateCommentLimit = 0,
							FollowPeopleLimit = 0,
							FollowTopicLimit = 0,
							LikeCommentLimit = 0,
							LikePostLimit = 0
						}
					};
			}
		}

		// TODO: Need to create a warm up function which does basic routine and increases over time until 2 weeks
		// TODO: MAKE SURE ALL USERS ARE BUSINESS ACCOUNTS AND USERS OVER 100 FOLLOWERS
		// TODO: BASE THEIR POSTING ON WHICH HOUR WAS MOST POPULAR
		public async Task Start(string accountId, string instagramAccountId)
		{
			var iteration = 0;
			while (true)
			{
				Console.WriteLine($"Started Agent Automation for {accountId}/{instagramAccountId}");
				try
				{
					var account = await _agentLogic.GetAccount(accountId, instagramAccountId);
					if (account == null)
						return;

					var profile = await _profileLogic.GetProfile(account.AccountId, account.Id);
					if (profile == null) return;

					if (account.UserLimits == null)
					{
						var userAccountDetails = await _authHandler.GetUserByUsername(account.AccountId);

						account.UserLimits = SetLimits(userAccountDetails.Roles.FirstOrDefault()
							.GetValueFromDescription<AuthTypes>());

						await _instagramAccountLogic.PartialUpdateInstagramAccount(account.AccountId, account.Id,
							new InstagramAccountModel {UserLimits = account.UserLimits});
					}

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

					var actionsContainerManager = new ActionsContainerManager();

					var likePostAct = _actionCommitFactory.Create(ActionType.LikePost, userStoreDetails);
					var likeCommentAct = _actionCommitFactory.Create(ActionType.LikeComment, userStoreDetails);
					var followAct = _actionCommitFactory.Create(ActionType.FollowUser, userStoreDetails);
					var createCommentAct = _actionCommitFactory.Create(ActionType.CreateCommentMedia, userStoreDetails);
					var postMediaAct = _actionCommitFactory.Create(ActionType.CreatePost, userStoreDetails)
						.ModifyOptions(new PostActionOptions(_postAnalyser, _s3BucketLogic));

					actionsContainerManager.AddAction(likePostAct, availableDate.AddMinutes(SecureRandom.Next(1, 4)),
						0.30);
					actionsContainerManager.AddAction(likeCommentAct, availableDate.AddMinutes(SecureRandom.Next(4)),
						0.25);
					actionsContainerManager.AddAction(followAct, availableDate.AddMinutes(SecureRandom.Next(1, 4)),
						0.20);
					actionsContainerManager.AddAction(createCommentAct,
						availableDate.AddMinutes(SecureRandom.Next(1, 4)), 0.10);
					actionsContainerManager.AddAction(postMediaAct, availableDate.AddMinutes(SecureRandom.Next(1, 5)),
						0.05);

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

					switch (account.AgentState)
					{
						case (int) AgentState.Running:
						{
							if (scheduledForUser != -1)
							{
								if (scheduledForUser > 100)
								{
									account.AgentState = (int) AgentState.Sleeping;
									await _instagramAccountLogic.PartialUpdateInstagramAccount(
										userStoreDetails.AccountId,
										userStoreDetails.InstagramAccountUser, new InstagramAccountModel
										{
											AgentState = account.AgentState,
										});
								}
							}

							var nominatedAction = actionsContainerManager.GetRandomAction();
							actionsContainerManager.AddWork(nominatedAction);
							await actionsContainerManager.RunAction();

							var actionFinished = actionsContainerManager.GetFinishedAction();
							if (actionFinished == null)
							{
								break;
							}

							foreach (var eventBody in actionFinished.DataObjects)
							{
								var timeSett = actionsContainerManager.FindActionLimit(actionFinished.ActionType);

								nextAvailableDate = _timelineLogic.PickAGoodTime(userStoreDetails.AccountId,
									userStoreDetails.InstagramAccountUser, actionFinished.ActionType);

								actionsContainerManager.HasMetTimeLimit();

								var availaDate = nextAvailableDate ?? DateTime.UtcNow;

								eventBody.ExecutionTime = availaDate.AddSeconds(timeSett.Max);

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

								if (res.HasCompleted)
								{

								}

								if (res.DailyLimitReached)
								{
									actionsContainerManager.TriggerAction(actionFinished.ActionType,
										DateTime.UtcNow.AddDays(1));
								}
								else if (res.HourlyLimitReached)
								{
									actionsContainerManager.TriggerAction(actionFinished.ActionType,
										DateTime.UtcNow.AddHours(1));
								}
							}

							break;
						}
						case (int) AgentState.Sleeping when scheduledForUser == -1:
							account.SleepTimeRemaining =
								DateTime.UtcNow.AddMinutes(SecureRandom.Next(25, 35) + SecureRandom.NextDouble());
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
								account.SleepTimeRemaining =
									DateTime.UtcNow.AddMinutes(SecureRandom.Next(25, 35) + SecureRandom.NextDouble());
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
							account.SleepTimeRemaining = DateTime.UtcNow.AddMinutes(20);
							account.AgentState = (int) AgentState.DeepSleep;
							await _instagramAccountLogic.PartialUpdateInstagramAccount(userStoreDetails.AccountId,
								userStoreDetails.InstagramAccountUser, new InstagramAccountModel
								{
									SleepTimeRemaining = account.SleepTimeRemaining,
									AgentState = account.AgentState
								});
							break;
						}
						case (int) AgentState.Challenge:
						{
							var items = _timelineLogic.GetScheduledEventsForUser(userStoreDetails.AccountId,
								userStoreDetails.InstagramAccountUser, 2000).ToList();
							items.ForEach(_ => { _timelineLogic.DeleteEvent(_.ItemId); });
							account.AgentState = (int) AgentState.AwaitingActionFromUser;
							await _instagramAccountLogic.PartialUpdateInstagramAccount(userStoreDetails.AccountId,
								userStoreDetails.InstagramAccountUser, new InstagramAccountModel
								{
									AgentState = account.AgentState,
								});
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
				finally
				{
					Console.WriteLine($"Ended Agent Automation for {accountId}/{instagramAccountId}");
				}

				await Task.Delay(TimeSpan.FromSeconds(1));
				Console.WriteLine($"Finished Iteration {iteration++}");
			}
		}

		#region GET TIMELINE DETAILS
		//		private ScheduleWindow GetDayCompletedActions(string accountId, string instagramId = null, int limit = 1000)
		//		{
		//			var todays = new List<ResultBase<TimelineItem>>();
		//			var backwards = _timelineLogic.GetFinishedEventsForUserByDate(accountId, DateTime.UtcNow, instaId: instagramId,
		//				limit: limit, timelineDateType: TimelineDateType.Backwards);
		//			var forward = _timelineLogic.GetFinishedEventsForUserByDate(accountId, DateTime.UtcNow, instaId: instagramId,
		//				limit: limit, timelineDateType: TimelineDateType.Forward);
		//
		//			if (backwards != null)
		//				todays.AddRange(backwards.Select(_ => new ResultBase<TimelineItem>
		//				{
		//					Response = new TimelineItem
		//					{
		//						ActionName = _.ActionName,
		//						EnqueueTime = _.SuccededAt,
		//						ItemId = _.ItemId,
		//						State = _.State,
		//						Url = _.Url,
		//						User = _.User
		//					},
		//					Message = _.Results,
		//					TimelineType = typeof(TimelineFinishedItem)
		//				}));
		//			if (forward != null)
		//				todays.AddRange(forward.Select(_ => new ResultBase<TimelineItem>
		//				{
		//					Response = new TimelineItem
		//					{
		//						ActionName = _.ActionName,
		//						EnqueueTime = _.SuccededAt,
		//						ItemId = _.ItemId,
		//						State = _.State,
		//						Url = _.Url,
		//						User = _.User
		//					},
		//					Message = _.Results,
		//					TimelineType = typeof(TimelineFinishedItem)
		//				}));
		//
		//			if (todays.Count <= 0) return new ScheduleWindow();
		//			{
		//				var schedulerHistory = todays.OrderBy(_ => _.Response.EnqueueTime).GroupBy(_ => new { Type = typeof(TimelineFinishedItem), _.Response.ActionName });
		//
		//				return new ScheduleWindow
		//				{
		//					All = schedulerHistory.Where(_ => _.Key.Type != typeof(TimelineDeletedItem) && _.Key?.ActionName?.Split('_')?[0].ToLower() != ActionType.CreatePost.GetDescription().ToLower()).SquashMe().ToList(),
		//					CreatePostActions = schedulerHistory.Where(_ => _.Key.Type != typeof(TimelineDeletedItem) && _.Key?.ActionName?.Split('_')?[0].ToLower() == ActionType.CreatePost.GetDescription().ToLower()).SquashMe().ToList(),
		//					CommentingActions = schedulerHistory.Where(_ => _.Key.Type != typeof(TimelineDeletedItem) && _.Key?.ActionName?.Split('_')?[0].ToLower() == ActionType.CreateCommentMedia.GetDescription().ToLower()).SquashMe().ToList(),
		//					FollowUserActions = schedulerHistory.Where(_ => _.Key.Type != typeof(TimelineDeletedItem) && _.Key?.ActionName?.Split('_')?[0].ToLower() == ActionType.FollowUser.GetDescription().ToLower()).SquashMe().ToList(),
		//					LikeMediaActions = schedulerHistory.Where(_ => _.Key.Type != typeof(TimelineDeletedItem) && _.Key?.ActionName?.Split('_')?[0].ToLower() == ActionType.LikePost.GetDescription().ToLower()).SquashMe().ToList()
		//				};
		//			}
		//		}
		//		private ScheduleWindow GetHourCompletedActions(string accountId, string instagramId = null, int limit = 1000)
		//		{
		//			var todays = new List<ResultBase<TimelineItem>>();
		//			var backwards = _timelineLogic.GetFinishedEventsForUserByDate(accountId, DateTime.UtcNow, endDate: DateTime.UtcNow.AddHours(-1), instaId: instagramId,
		//				limit: limit, timelineDateType: TimelineDateType.Backwards);
		//			var forward = _timelineLogic.GetFinishedEventsForUserByDate(accountId, DateTime.UtcNow, endDate: DateTime.UtcNow.AddHours(-1), instaId: instagramId,
		//				limit: limit, timelineDateType: TimelineDateType.Forward);
		//
		//			if (backwards != null)
		//				todays.AddRange(backwards.Select(_=>new ResultBase<TimelineItem>
		//				{
		//					Response = new TimelineItem
		//					{
		//						ActionName = _.ActionName,
		//						EnqueueTime = _.SuccededAt,
		//						ItemId = _.ItemId,
		//						State = _.State,
		//						Url = _.Url,
		//						User = _.User
		//					},
		//					Message = _.Results,
		//					TimelineType = typeof(TimelineFinishedItem)
		//				}));
		//			if (forward != null)
		//				todays.AddRange(forward.Select(_=>new ResultBase<TimelineItem>
		//				{
		//					Response = new TimelineItem
		//					{
		//						ActionName = _.ActionName,
		//						EnqueueTime = _.SuccededAt,
		//						ItemId = _.ItemId,
		//						State = _.State,
		//						Url = _.Url,
		//						User = _.User
		//					},
		//					Message = _.Results,
		//					TimelineType = typeof(TimelineFinishedItem)
		//				}));
		//
		//
		//			if (todays.Count <= 0) return new ScheduleWindow();
		//			{
		//				var schedulerHistory = todays.OrderBy(_ => _.Response.EnqueueTime).GroupBy(_ => new { Type = typeof(TimelineFinishedItem), _.Response.ActionName });
		//
		//				return new ScheduleWindow
		//				{
		//					All = schedulerHistory.Where(_ => !string.Equals(_.Key?.ActionName?.Split('_')?[0], ActionType.CreatePost.GetDescription(), StringComparison.CurrentCultureIgnoreCase)).SquashMe().ToList(),
		//					CreatePostActions = schedulerHistory.Where(_ => string.Equals(_.Key?.ActionName?.Split('_')?[0], ActionType.CreatePost.GetDescription(), StringComparison.CurrentCultureIgnoreCase)).SquashMe().ToList(),
		//					CommentingActions = schedulerHistory.Where(_ => string.Equals(_.Key?.ActionName?.Split('_')?[0], ActionType.CreateCommentMedia.GetDescription(), StringComparison.CurrentCultureIgnoreCase)).SquashMe().ToList(),
		//					FollowUserActions = schedulerHistory.Where(_ => string.Equals(_.Key?.ActionName?.Split('_')?[0], ActionType.FollowUser.GetDescription(), StringComparison.CurrentCultureIgnoreCase)).SquashMe().ToList(),
		//					LikeMediaActions = schedulerHistory.Where(_ => string.Equals(_.Key?.ActionName?.Split('_')?[0], ActionType.LikePost.GetDescription(), StringComparison.CurrentCultureIgnoreCase)).SquashMe().ToList(),
		//					LikeCommentActions = schedulerHistory.Where(_ => string.Equals(_.Key?.ActionName?.Split('_')?[0], ActionType.LikeComment.GetDescription(), StringComparison.CurrentCultureIgnoreCase)).SquashMe().ToList()
		//				};
		//			}
		//		}

		// private ScheduleWindow GetTodaysScheduleWindow(string accountId, string instagramId = null, int limit = 1000)
		// {
		// 	var _locker = new object();
		// 	lock (_locker)
		// 	{
		// 		var todays = new List<ResultBase<TimelineItem>>();
		// 		var backwards = _timelineLogic.GetAllEventsForUser(accountId, DateTime.UtcNow, instaId: instagramId,
		// 			limit: limit, timelineDateType: TimelineDateType.Backwards);
		// 		var forward = _timelineLogic.GetAllEventsForUser(accountId, DateTime.UtcNow, instaId: instagramId,
		// 			limit: limit, timelineDateType: TimelineDateType.Forward);
		//
		// 		if (backwards != null)
		// 			todays.AddRange(backwards);
		// 		if (forward != null)
		// 			todays.AddRange(forward);
		//
		// 		if (todays.Count <= 0) return null;
		// 		var schedulerHistory = todays.OrderBy(_ => _.Response.EnqueueTime).GroupBy(_ => new { _.TimelineType, _.Response.ActionName });
		//
		// 		return new ScheduleWindow
		// 		{
		// 			All = schedulerHistory.Where(_ => !string.Equals(_.Key?.ActionName?.Split('_')?[0], ActionType.CreatePost.GetDescription(), StringComparison.CurrentCultureIgnoreCase)).SquashMe().ToList(),
		// 			CreatePostActions = schedulerHistory.Where(_ => string.Equals(_.Key?.ActionName?.Split('_')?[0], ActionType.CreatePost.GetDescription(), StringComparison.CurrentCultureIgnoreCase)).SquashMe().ToList(),
		// 			CommentingActions = schedulerHistory.Where(_ => string.Equals(_.Key?.ActionName?.Split('_')?[0], ActionType.CreateCommentMedia.GetDescription(), StringComparison.CurrentCultureIgnoreCase)).SquashMe().ToList(),
		// 			FollowUserActions = schedulerHistory.Where(_ => string.Equals(_.Key?.ActionName?.Split('_')?[0], ActionType.FollowUser.GetDescription(), StringComparison.CurrentCultureIgnoreCase)).SquashMe().ToList(),
		// 			LikeMediaActions = schedulerHistory.Where(_ => string.Equals(_.Key?.ActionName?.Split('_')?[0], ActionType.LikePost.GetDescription(), StringComparison.CurrentCultureIgnoreCase)).SquashMe().ToList(),
		// 			LikeCommentActions = schedulerHistory.Where(_ => string.Equals(_.Key?.ActionName?.Split('_')?[0], ActionType.LikeComment.GetDescription(), StringComparison.CurrentCultureIgnoreCase)).SquashMe().ToList()
		// 		};
		// 	}
		// }

		//		private ScheduleWindow GetLastHoursScheduleWindow(string accountId, string instagramId = null, int limit = 1000)
		//		{
		//			var todays = new List<ResultBase<TimelineItem>>();
		//			var backwards = _timelineLogic.GetAllEventsForUser(accountId, DateTime.UtcNow, endDate: DateTime.UtcNow.AddHours(-1),
		//				instaId: instagramId, limit: limit, timelineDateType: TimelineDateType.Backwards);
		//			var forward = _timelineLogic.GetAllEventsForUser(accountId, DateTime.UtcNow, endDate: DateTime.UtcNow.AddHours(-1),
		//				instaId: instagramId, limit: limit, timelineDateType: TimelineDateType.Forward);
		//			
		//			if(backwards!=null)
		//				todays.AddRange(backwards);
		//			if(forward!=null)
		//				todays.AddRange(forward);
		//			if (todays.Count <= 0) return new ScheduleWindow();
		//			var schedulerHistory = todays.OrderBy(_ => _.Response.EnqueueTime).GroupBy(_ => new { _.TimelineType, _.Response.ActionName });
		//
		//			return new ScheduleWindow
		//			{
		//				All = schedulerHistory.SquashMe().ToList(),
		//				CreatePostActions = schedulerHistory.Where(_ => _.Key.TimelineType != typeof(TimelineDeletedItem) && _.Key?.ActionName?.Split('_')?[0].ToLower() == ActionType.CreatePost.GetDescription().ToLower()).SquashMe().ToList(),
		//				CommentingActions = schedulerHistory.Where(_ => _.Key.TimelineType != typeof(TimelineDeletedItem) && _.Key?.ActionName?.Split('_')?[0].ToLower() == ActionType.CreateCommentMedia.GetDescription().ToLower()).SquashMe().ToList(),
		//				FollowUserActions = schedulerHistory.Where(_ => _.Key.TimelineType != typeof(TimelineDeletedItem) && _.Key?.ActionName?.Split('_')?[0].ToLower() == ActionType.FollowUser.GetDescription().ToLower()).SquashMe().ToList(),
		//				LikeMediaActions = schedulerHistory.Where(_ => _.Key.TimelineType != typeof(TimelineDeletedItem) && _.Key?.ActionName?.Split('_')?[0].ToLower() == ActionType.LikePost.GetDescription().ToLower()).SquashMe().ToList()
		//			};
		//		}
		//		private List<ResultBase<TimelineItem>> GetEveryActionForToday (string accid, ActionType action, string instaacc = null, int limit = 5000, bool isHourly = false)
		//		{
		//			var schedule = new ScheduleWindow();
		//			schedule = isHourly ? GetHourCompletedActions(accid, instaacc, limit) : GetDayCompletedActions(accid, instaacc, limit);
		//
		//			switch (action)
		//			{
		//				case ActionType.LikePost:
		//					return schedule.LikeMediaActions;
		//				case ActionType.CreateCommentMedia:
		//					return schedule.CommentingActions;
		//				case ActionType.CreatePost:
		//					return schedule.CreatePostActions;
		//				case ActionType.FollowUser:
		//					return schedule.FollowUserActions;
		//				case ActionType.LikeComment:
		//					return schedule.LikeCommentActions;
		//			}
		//
		//			return null;
		//		}
		//		
		#endregion
	}
}
