using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoreLinq.Extensions;
using Quarkless.Analyser;
using Quarkless.Logic.Agent;
using Quarkless.Logic.Services.Automation.Actions.ActionManager;
using Quarkless.Logic.Services.Automation.Factory.FactoryManager;
using Quarkless.Models.Actions.Interfaces;
using Quarkless.Models.Agent.Interfaces;
using Quarkless.Models.Auth.Enums;
using Quarkless.Models.Auth.Interfaces;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Common.Extensions;
using Quarkless.Models.Common.Objects;
using Quarkless.Models.ContentInfo.Interfaces;
using Quarkless.Models.Heartbeat.Interfaces;
using Quarkless.Models.InstagramAccounts;
using Quarkless.Models.InstagramAccounts.Enums;
using Quarkless.Models.InstagramAccounts.Interfaces;
using Quarkless.Models.Library.Interfaces;
using Quarkless.Models.Profile.Interfaces;
using Quarkless.Models.RequestBuilder.Interfaces;
using Quarkless.Models.Services.Automation.Enums.Actions.ActionType;
using Quarkless.Models.Services.Automation.Enums.Actions.StrategyType;
using Quarkless.Models.Services.Automation.Interfaces;
using Quarkless.Models.Services.Automation.Models.ActionOptions;
using Quarkless.Models.Services.Automation.Models.Agent;
using Quarkless.Models.Services.Automation.Models.StrategySettings;
using Quarkless.Models.Storage.Interfaces;
using Quarkless.Models.Timeline;
using Quarkless.Models.Timeline.Enums;
using Quarkless.Models.Timeline.Interfaces;
using Quarkless.Models.Timeline.TaskScheduler;

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
		private readonly IActionFactory _actionFactory;
		private readonly IAgentLogic _agentLogic;
		private readonly IActionCommitFactory _actionCommitFactory;
		public AgentManager(IInstagramAccountLogic instagramAccountLogic, IProfileLogic profileLogic,
			ITimelineLogic timelineLogic, ITimelineEventLogLogic eventLogLogic,IAuthHandler authHandler, 
			ILibraryLogic libraryLogic, IS3BucketLogic s3Bucket, IPostAnalyser postAnalyser, IActionFactory actionFactory)
		{
			_timelineLogic = timelineLogic;
			_timelineEventLogs = eventLogLogic;
			_instagramAccountLogic = instagramAccountLogic;
			_profileLogic = profileLogic;
			_authHandler = authHandler;
			_libraryLogic = libraryLogic;
			_s3BucketLogic = s3Bucket;
			_postAnalyser = postAnalyser;
			_actionFactory = actionFactory;
			_agentLogic = new AgentLogic(instagramAccountLogic);
		}
		#endregion

		private async Task<AddEventResponse> AddToTimeline(TimelineEventModel @event, Limits limits)
		{
			if (@event == null || limits == null) return null;
			try
			{
				var actionBase = @event.ActionName.Split('_')[0].ToLower().GetValueFromDescription<ActionType>();
				var completedActionsDaily = await SuccessfulActionCount(actionBase, @event.Data.User.OAccountId,
					@event.Data.User.OInstagramAccountUser);
				var completedActionsHourly = await SuccessfulActionCount(actionBase, @event.Data.User.OAccountId,
					@event.Data.User.OInstagramAccountUser, isHourly:true);

				switch (actionBase)
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

				_timelineLogic.AddEventToTimeline(@event.ActionName, @event.Data, @event.ExecutionTime);
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
			
			//var logs = await _timelineEventLogs.GetLogsForUser(accountId, instagramAccountId, limit);
			var logs = await _timelineEventLogs.GetLogsForUser(accountId, instagramAccountId, limit);
			var startDate = DateTime.UtcNow;
			var endDate = !isHourly ? startDate.AddDays(-1).AddTicks(1) : startDate.AddHours(-1).AddTicks(1);

			return logs.Count(x => 
				x.Status == TimelineEventStatus.Success
					&& (x.DateAdded <= startDate && x.DateAdded >= endDate)
					&&  x.ActionType == actionName
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

		public async Task Start(string accountId, string instagramAccountId)
		{
			//todo: possibly use delegate based schedule instead of rest model, will launch app faster as no auth will need to take place
			//todo: it will also perform action faster
			var account = await _agentLogic.GetAccount(accountId, instagramAccountId);
			if (account == null)
				return;

			var res = await _actionCommitFactory.Create(Models.Actions.Enums.ActionType.CreatePost,
				new UserStoreDetails { })
				.ModifyOptions(new Models.Actions.Factory.Action_Options.PostActionOptions(_postAnalyser, _s3BucketLogic))
				.PushAsync(DateTimeOffset.Now);

			foreach (var resultsDataObject in res.Results.DataObjects)
			{
				_timelineLogic.AddEventToTimeline(new EventActionOptions
				{
					ActionType = (int)res.Results.ActionType,
					DataObject = new EventActionOptions.EventBody
					{
						Body = resultsDataObject.Body,
						BodyType = resultsDataObject.BodyType
					},
					ExecutionTime = resultsDataObject.ExecutionTime,
					User = res.Results.User
				});
			}
		}

		// TODO: Need to create a warm up function which does basic routine and increases over time until 2 weeks
		// TODO: MAKE SURE ALL USERS ARE BUSINESS ACCOUNTS AND USERS OVER 100 FOLLOWERS
		// TODO: BASE THEIR POSTING ON WHICH HOUR WAS MOST POPULAR
		public async Task Begin(string userId, string instagramAccountId)
		{
			var shortInstagram = await _instagramAccountLogic.GetInstagramAccountShort(userId, instagramAccountId);
			while (true) {
				if (shortInstagram != null)
				{
					var userStoreDetails = new UserStoreDetails();
					try
					{
						#region Token Stuff

						var acc = (await _authHandler.GetUserByUsername(shortInstagram.AccountId));
						var expTime = acc.Claims?.Where(s => s.Type == "exp")?.SingleOrDefault();
						var accessToken = acc.Tokens.SingleOrDefault(_ => _.Name == "access_token")?.Value;
						var refreshToken = acc.Tokens.SingleOrDefault(_ => _.Name == "refresh_token")?.Value;
						var idToken = acc.Tokens.SingleOrDefault(_ => _.Name == "id_token")?.Value;

						if (shortInstagram.UserLimits == null)
						{
							shortInstagram.UserLimits = SetLimits(acc.Roles.FirstOrDefault().GetValueFromDescription<AuthTypes>());
						}

						userStoreDetails.ORefreshToken = refreshToken;

						if (expTime != null)
						{
							var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
							var today = epoch.AddSeconds(long.Parse(expTime.Value));
							if (DateTime.UtcNow > today.AddMinutes(-20))
							{
								var res = await _authHandler.RefreshLogin(refreshToken, acc.UserName);
								shortInstagram.UserLimits = SetLimits(acc.Roles.FirstOrDefault().GetValueFromDescription<AuthTypes>());
								await _instagramAccountLogic.PartialUpdateInstagramAccount(shortInstagram.AccountId, shortInstagram.Id, new InstagramAccountModel { UserLimits = shortInstagram.UserLimits });
								accessToken = res.Results.AccessToken;
								idToken = res.Results.IdToken;

								userStoreDetails.AddUpdateUser(shortInstagram.AccountId, shortInstagram.Id, idToken);
								userStoreDetails.OInstagramAccountUsername = shortInstagram.Username;
								var items = _timelineLogic.GetScheduledEventsForUser(userStoreDetails.OAccountId, userStoreDetails.OInstagramAccountUser, 1000).ToList();

								items.ForEach(_ =>
								{
									_timelineLogic.DeleteEvent(_.ItemId);
									_.User = userStoreDetails;
									_.Rest.User = userStoreDetails;
									_timelineLogic.AddEventToTimeline(_.ActionName, _.Rest, _.EnqueueTime.Value.AddSeconds(15));
								});
							}
							else
							{
								userStoreDetails.AddUpdateUser(shortInstagram.AccountId, shortInstagram.Id, idToken);
								userStoreDetails.OInstagramAccountUsername = shortInstagram.Username;
							}
						}

						#endregion

						var profile = await _profileLogic.GetProfile(userStoreDetails.OAccountId, userStoreDetails.OInstagramAccountUser);
						if (profile == null) return;
						
						userStoreDetails.ShortInstagram = shortInstagram;
						userStoreDetails.Profile = profile;
						userStoreDetails.MessagesTemplates = await _libraryLogic.GetSavedMessagesForUser(shortInstagram.Id);
						
						var nextAvailableDate = _timelineLogic.PickAGoodTime(userStoreDetails.OAccountId, userStoreDetails.OInstagramAccountUser);
						
						#region Unfollow Section
						if (shortInstagram.LastPurgeCycle == null)
						{
							await _instagramAccountLogic.PartialUpdateInstagramAccount(userStoreDetails.OAccountId, userStoreDetails.OInstagramAccountUser,
								new InstagramAccountModel
								{
									LastPurgeCycle = DateTime.UtcNow.AddHours(SecureRandom.Next(_unfollowPurgeCycle.Min, _unfollowPurgeCycle.Max))
								});
						}
						else
						{
							if (DateTime.UtcNow > shortInstagram.LastPurgeCycle)
							{
								if (shortInstagram.FollowingCount != null)
								{
									var res = _actionFactory.Commit(ActionType.UnFollowUser)
										.IncludeStrategy(new UnFollowStrategySettings
										{
											UnFollowStrategy = UnFollowStrategyType.LeastEngagingN,
											NumberOfUnfollows = (int)(shortInstagram.FollowingCount.Value * UNFOLLOW_AMOUNT),
											OffsetPerAction = TimeSpan.FromSeconds(SecureRandom.Next(UnfollowActionOptions.TimeFrameSeconds.Min,
												UnfollowActionOptions.TimeFrameSeconds.Max))
										})
										.IncludeUser(userStoreDetails)
										.Push(new UnfollowActionOptions(nextAvailableDate.AddSeconds(UnfollowActionOptions.TimeFrameSeconds.Max)));

									if (res.IsSuccessful)
									{
										foreach (var r in res.Results)
											_timelineLogic.AddEventToTimeline(r.ActionName, r.Data, r.ExecutionTime);

										await _instagramAccountLogic.PartialUpdateInstagramAccount(userStoreDetails.OAccountId, userStoreDetails.OInstagramAccountUser,
											new InstagramAccountModel
											{
												LastPurgeCycle = DateTime.UtcNow.AddHours(SecureRandom.Next(_unfollowPurgeCycle.Min, _unfollowPurgeCycle.Max))
											});
									}
								}
							}
						}
						#endregion
						//var totalForUser = GetTodaysScheduleWindow(userStoreDetails.OAccountId, userStoreDetails.OInstagramAccountUser)?.All?.ToList();
						
						var scheduledForUser = ScheduledCount(userStoreDetails.OAccountId,
												userStoreDetails.OInstagramAccountUser);

						#region Action Initialise
						var actionsContainerManager = new ActionsContainerManager();

						var likeAction = _actionFactory.Commit(ActionType.LikePost)
							.IncludeStrategy(new LikeStrategySettings()
							{
								LikeStrategy = LikeStrategyType.Default,
							})
							.IncludeUser(userStoreDetails);

						var postAction = _actionFactory.Commit(ActionType.CreatePost)
							.IncludeStrategy(new ImageStrategySettings
							{
								ImageStrategyType = ImageStrategyType.Default
							})
							.IncludeStorage(_s3BucketLogic)
							.IncludeUser(userStoreDetails);

						var followAction = _actionFactory.Commit(ActionType.FollowUser)
							.IncludeStrategy(new FollowStrategySettings
							{
								FollowStrategy = FollowStrategyType.Default,
							})
							.IncludeUser(userStoreDetails);

						var commentAction = _actionFactory.Commit(ActionType.CreateCommentMedia)
							.IncludeStrategy(new CommentingStrategySettings
							{
								CommentingStrategy = CommentingStrategy.Default,
							})
							.IncludeUser(userStoreDetails);

						var likeCommentAction = _actionFactory.Commit(ActionType.LikeComment)
							.IncludeStrategy(new LikeStrategySettings())
							.IncludeUser(userStoreDetails);

						var sendMessageAction = _actionFactory.Commit(ActionType.SendDirectMessage)
							.IncludeUser(userStoreDetails);

						//Initial Execution
						var likeScheduleOptions = new LikeActionOptions(nextAvailableDate.AddMinutes(SecureRandom.Next(1, 4)), LikeActionType.Any);
						var postScheduleOptions = new PostActionOptions(nextAvailableDate.AddMinutes(SecureRandom.Next(1, 5)), _postAnalyser) { ImageFetchLimit = 20 };
						var followScheduleOptions = new FollowActionOptions(nextAvailableDate.AddMinutes(SecureRandom.Next(1, 4)), FollowActionType.Any);
						var commentScheduleOptions = new CommentingActionOptions(nextAvailableDate.AddMinutes(SecureRandom.Next(1, 4)), CommentingActionType.Any);
						var likeCommentActionOptions = new LikeCommentActionOptions(nextAvailableDate.AddMinutes(SecureRandom.Next(4)), LikeCommentActionType.Any);
						//var sendMessageScheduleoptions = new SendDirectMessageActionOptions(nextAvailableDate.AddMinutes(SecureRandom.Next(1, 5)), MessagingReachType.Any, 1, _postAnalyser);

						//actionsContainerManager.AddAction(sendMessageAction, sendMessageScheduleoptions, 0.05);
						actionsContainerManager.AddAction(postAction, postScheduleOptions, 0.05);
						actionsContainerManager.AddAction(likeAction, likeScheduleOptions, 0.25);
						actionsContainerManager.AddAction(followAction, followScheduleOptions, 0.20);
						actionsContainerManager.AddAction(commentAction, commentScheduleOptions, 0.15);
						actionsContainerManager.AddAction(likeCommentAction, likeCommentActionOptions, 0.25);

						#endregion

						#region Agent State
						if (shortInstagram == null) return;
						if (shortInstagram.AgentState == (int)AgentState.NotStarted)
						{
							shortInstagram.AgentState = (int)AgentState.Running;
							await _instagramAccountLogic.PartialUpdateInstagramAccount(userStoreDetails.OAccountId, userStoreDetails.OInstagramAccountUser, new InstagramAccountModel
							{
								AgentState = shortInstagram.AgentState,
							});
						}
						switch (shortInstagram.AgentState)
						{
							case (int)AgentState.Running:
								{
									if (scheduledForUser != -1)
									{
										if (scheduledForUser > 100)
										{
											shortInstagram.AgentState = (int)AgentState.Sleeping;
											await _instagramAccountLogic.PartialUpdateInstagramAccount(userStoreDetails.OAccountId, userStoreDetails.OInstagramAccountUser, new InstagramAccountModel
											{
												AgentState = shortInstagram.AgentState,
											});
										}
									}
									var nominatedAction = actionsContainerManager.GetRandomAction();
									actionsContainerManager.AddWork(nominatedAction);
									actionsContainerManager.RunAction();
									var finishedAction = actionsContainerManager.GetFinishedActions()?.DistinctBy(d => d.Data);
									finishedAction?.ForEach(async _ =>
									{
										var actionName = _.ActionName.Split('_')[0].ToLower();
										var actionType = actionName.GetValueFromDescription<ActionType>();
										var timeSett = actionsContainerManager.FindActionLimit(actionType);

										nextAvailableDate = _timelineLogic.PickAGoodTime(userStoreDetails.OAccountId, userStoreDetails.OInstagramAccountUser, actionName.GetValueFromDescription<ActionType>());
										actionsContainerManager.HasMetTimeLimit();
										if (nextAvailableDate == null) return;
										if (timeSett != null)
											_.ExecutionTime = nextAvailableDate.AddSeconds(timeSett.Value.Max);
										var res = await AddToTimeline(_, shortInstagram.UserLimits);
										if (res.HasCompleted)
										{

										}
										if (res.DailyLimitReached)
										{
											actionsContainerManager.TriggerAction(actionName.GetValueFromDescription<ActionType>(), DateTime.UtcNow.AddDays(1));
										}
										else if (res.HourlyLimitReached)
										{
											actionsContainerManager.TriggerAction(actionName.GetValueFromDescription<ActionType>(), DateTime.UtcNow.AddHours(1));
										}
									});
									break;
								}
							case (int)AgentState.Sleeping when scheduledForUser == -1:
								shortInstagram.SleepTimeRemaining = DateTime.UtcNow.AddMinutes(SecureRandom.Next(25, 35) + SecureRandom.NextDouble());
								shortInstagram.AgentState = (int)AgentState.DeepSleep;
								await _instagramAccountLogic.PartialUpdateInstagramAccount(userStoreDetails.OAccountId, userStoreDetails.OInstagramAccountUser, new InstagramAccountModel
								{
									SleepTimeRemaining = shortInstagram.SleepTimeRemaining,
									AgentState = shortInstagram.AgentState
								});
								break;
							case (int)AgentState.Sleeping:
								{
									if (scheduledForUser <= 10)
									{
										shortInstagram.SleepTimeRemaining = DateTime.UtcNow.AddMinutes(SecureRandom.Next(25, 35) + SecureRandom.NextDouble());
										shortInstagram.AgentState = (int)AgentState.DeepSleep;

										await _instagramAccountLogic.PartialUpdateInstagramAccount(userStoreDetails.OAccountId, userStoreDetails.OInstagramAccountUser, new InstagramAccountModel
										{
											SleepTimeRemaining = shortInstagram.SleepTimeRemaining,
											AgentState = shortInstagram.AgentState
										});
									}
									break;
								}
							case (int)AgentState.Blocked:
								{
									var items = _timelineLogic.GetScheduledEventsForUser(userStoreDetails.OAccountId, userStoreDetails.OInstagramAccountUser, 2000).ToList();
									items.ForEach(_ =>
									{
										_timelineLogic.DeleteEvent(_.ItemId);
									});
									shortInstagram.SleepTimeRemaining = DateTime.UtcNow.AddHours(6);
									shortInstagram.AgentState = (int)AgentState.DeepSleep;
									await _instagramAccountLogic.PartialUpdateInstagramAccount(userStoreDetails.OAccountId, userStoreDetails.OInstagramAccountUser, new InstagramAccountModel
									{
										SleepTimeRemaining = shortInstagram.SleepTimeRemaining,
										AgentState = shortInstagram.AgentState
									});
									break;
								}
							case (int)AgentState.Challenge:
								{
									var items = _timelineLogic.GetScheduledEventsForUser(userStoreDetails.OAccountId, userStoreDetails.OInstagramAccountUser, 2000).ToList();
									items.ForEach(_ =>
									{
										_timelineLogic.DeleteEvent(_.ItemId);
									});
									shortInstagram.AgentState = (int)AgentState.AwaitingActionFromUser;
									await _instagramAccountLogic.PartialUpdateInstagramAccount(userStoreDetails.OAccountId, userStoreDetails.OInstagramAccountUser, new InstagramAccountModel
									{
										AgentState = shortInstagram.AgentState,
									});
									break;
								}
							case (int)AgentState.DeepSleep when shortInstagram.SleepTimeRemaining.HasValue:
								{
									if (DateTime.UtcNow > shortInstagram.SleepTimeRemaining.Value)
									{
										shortInstagram.AgentState = (int)AgentState.Running;
										await _instagramAccountLogic.PartialUpdateInstagramAccount(userStoreDetails.OAccountId, userStoreDetails.OInstagramAccountUser, new InstagramAccountModel
										{
											AgentState = shortInstagram.AgentState,
										});
									}
									break;
								}
							case (int)AgentState.DeepSleep:
								shortInstagram.AgentState = (int)AgentState.Running;
								await _instagramAccountLogic.PartialUpdateInstagramAccount(userStoreDetails.OAccountId, userStoreDetails.OInstagramAccountUser, new InstagramAccountModel
								{
									AgentState = shortInstagram.AgentState,
								});
								break;
							case (int)AgentState.Stopped:
								break;
						}
						#endregion
					}
					catch (Exception ee)
					{
						Console.WriteLine(ee.Message);
					}
				}

				await Task.Delay(TimeSpan.FromSeconds(1));
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

		private ScheduleWindow GetTodaysScheduleWindow(string accountId, string instagramId = null, int limit = 1000)
		{
			var _locker = new object();
			lock (_locker)
			{
				var todays = new List<ResultBase<TimelineItem>>();
				var backwards = _timelineLogic.GetAllEventsForUser(accountId, DateTime.UtcNow, instaId: instagramId,
					limit: limit, timelineDateType: TimelineDateType.Backwards);
				var forward = _timelineLogic.GetAllEventsForUser(accountId, DateTime.UtcNow, instaId: instagramId,
					limit: limit, timelineDateType: TimelineDateType.Forward);

				if (backwards != null)
					todays.AddRange(backwards);
				if (forward != null)
					todays.AddRange(forward);

				if (todays.Count <= 0) return null;
				var schedulerHistory = todays.OrderBy(_ => _.Response.EnqueueTime).GroupBy(_ => new { _.TimelineType, _.Response.ActionName });

				return new ScheduleWindow
				{
					All = schedulerHistory.Where(_ => !string.Equals(_.Key?.ActionName?.Split('_')?[0], ActionType.CreatePost.GetDescription(), StringComparison.CurrentCultureIgnoreCase)).SquashMe().ToList(),
					CreatePostActions = schedulerHistory.Where(_ => string.Equals(_.Key?.ActionName?.Split('_')?[0], ActionType.CreatePost.GetDescription(), StringComparison.CurrentCultureIgnoreCase)).SquashMe().ToList(),
					CommentingActions = schedulerHistory.Where(_ => string.Equals(_.Key?.ActionName?.Split('_')?[0], ActionType.CreateCommentMedia.GetDescription(), StringComparison.CurrentCultureIgnoreCase)).SquashMe().ToList(),
					FollowUserActions = schedulerHistory.Where(_ => string.Equals(_.Key?.ActionName?.Split('_')?[0], ActionType.FollowUser.GetDescription(), StringComparison.CurrentCultureIgnoreCase)).SquashMe().ToList(),
					LikeMediaActions = schedulerHistory.Where(_ => string.Equals(_.Key?.ActionName?.Split('_')?[0], ActionType.LikePost.GetDescription(), StringComparison.CurrentCultureIgnoreCase)).SquashMe().ToList(),
					LikeCommentActions = schedulerHistory.Where(_ => string.Equals(_.Key?.ActionName?.Split('_')?[0], ActionType.LikeComment.GetDescription(), StringComparison.CurrentCultureIgnoreCase)).SquashMe().ToList()
				};
			}
		}

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
