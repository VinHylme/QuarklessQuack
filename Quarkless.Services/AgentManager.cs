﻿using Quarkless.Services.ActionBuilders.EngageActions;
using Quarkless.Services.Factories;
using Quarkless.Services.Interfaces;
using Quarkless.Services.Interfaces.Actions;
using Quarkless.Services.StrategyBuilders;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models;
using QuarklessContexts.Models.Timeline;
using QuarklessLogic.ServicesLogic.TimelineServiceLogic.TimelineLogic;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using QuarklessLogic.Logic.InstagramAccountLogic;
using QuarklessLogic.Logic.AuthLogic.Auth;
using QuarklessContexts.Models.InstagramAccounts;
using QuarklessLogic.Logic.ProfileLogic;
using QuarklessLogic.ServicesLogic.HeartbeatLogic;
using MoreLinq;
using QuarklessLogic.ServicesLogic.AgentLogic;
using System.Collections.Async;
using QuarklessContexts.Models.UserAuth.AuthTypes;
using QuarklessContexts.Enums;

namespace Quarkless.Services
{
	#region ScheduleWindow Class
	class ScheduleWindow
	{
		public List<ResultBase<TimelineItem>> CreatePostActions;
		public List<ResultBase<TimelineItem>> CommentingActions;
		public List<ResultBase<TimelineItem>> LikeMediaActions;
		public List<ResultBase<TimelineItem>> LikeCommentActions;
		public List<ResultBase<TimelineItem>> FollowUserActions;
		public List<ResultBase<TimelineItem>> All;
		public ScheduleWindow()
		{
			All = new List<ResultBase<TimelineItem>>();
			CreatePostActions = new List<ResultBase<TimelineItem>>();
			LikeMediaActions = new List<ResultBase<TimelineItem>>();
			FollowUserActions = new List<ResultBase<TimelineItem>>();
			LikeCommentActions = new List<ResultBase<TimelineItem>>();
			CommentingActions = new List<ResultBase<TimelineItem>>();
		}
	}
	#endregion
	public sealed class AgentManager : IAgentManager
	{
		private readonly IInstagramAccountLogic _instagramAccountLogic;
		private readonly IProfileLogic _profileLogic;
		private readonly IContentManager _contentManager;
		private readonly ITimelineLogic _timelineLogic;
		private readonly IAuthHandler _authHandler;
		private readonly IHeartbeatLogic _heartbeatLogic;
		private readonly IAgentLogic _agentLogic;
		public AgentManager(IInstagramAccountLogic instagramAccountLogic, IProfileLogic profileLogic ,IContentManager contentManager,
			ITimelineLogic timelineLogic,IHeartbeatLogic heartbeatLogic, IAuthHandler authHandler, IAgentLogic agentLogic)
		{
			_agentLogic = agentLogic;
			_heartbeatLogic = heartbeatLogic;
			_timelineLogic = timelineLogic;
			_instagramAccountLogic = instagramAccountLogic;
			_profileLogic = profileLogic;
			_contentManager = contentManager;
			_authHandler = authHandler;
		}

		private AddEventResponse AddToTimeline(TimelineEventModel events, Limits limits)
		{
			if (events == null || limits == null) return null;
			try
			{
				var actionBase = events.ActionName.Split('_')[0].ToLower().GetValueFromDescription<ActionType>();
				var completedActionsHourly = GetEveryActionForToday(events.Data.User.OAccountId,actionBase,events.Data.User.OInstagramAccountUser,isHourly:true);
				var completedActionsDaily = GetEveryActionForToday(events.Data.User.OAccountId, actionBase, events.Data.User.OInstagramAccountUser);
				if(completedActionsDaily!=null && completedActionsHourly != null)
				{
					switch (actionBase)
					{
						case ActionType.CreatePost when completedActionsDaily.Count >= limits.DailyLimits.CreatePostLimit:
							return new AddEventResponse
							{
								Event = events,
								ContainsErrors = false,
								HasCompleted = false,
								DailyLimitReached = true
							};
						case ActionType.CreatePost when completedActionsHourly.Count >= limits.HourlyLimits.CreatePostLimit:
							return new AddEventResponse
							{
								Event = events,
								ContainsErrors = false,
								HasCompleted = false,
								DailyLimitReached = true
							};
						case ActionType.CreateCommentMedia when completedActionsDaily.Count >= limits.DailyLimits.CreateCommentLimit:
							return new AddEventResponse
							{
								Event = events,
								ContainsErrors = false,
								HasCompleted = false,
								DailyLimitReached = true
							};
						case ActionType.CreateCommentMedia when completedActionsHourly.Count >= limits.HourlyLimits.CreateCommentLimit:
							return new AddEventResponse
							{
								Event = events,
								ContainsErrors = false,
								HasCompleted = false,
								HourlyLimitReached = true
							};
						case ActionType.LikePost when completedActionsDaily.Count >= limits.DailyLimits.LikePostLimit:
							return new AddEventResponse
							{
								Event = events,
								ContainsErrors = false,
								HasCompleted = false,
								DailyLimitReached = true
							};
						case ActionType.LikePost:
						{
							if (completedActionsHourly.Count >= limits.HourlyLimits.LikePostLimit)
							{
								new AddEventResponse
								{
									Event = events,
									ContainsErrors = false,
									HasCompleted = false,
									HourlyLimitReached = true
								};
							}

							break;
						}
						case ActionType.LikeComment when completedActionsDaily.Count >= limits.DailyLimits.LikeCommentLimit:
							return new AddEventResponse
							{
								Event = events,
								ContainsErrors = false,
								HasCompleted = false,
								DailyLimitReached = true
							};
						case ActionType.LikeComment when completedActionsHourly.Count >= limits.HourlyLimits.LikeCommentLimit:
							return new AddEventResponse
							{
								Event = events,
								ContainsErrors = false,
								HasCompleted = false,
								HourlyLimitReached = true
							};
						case ActionType.FollowUser when completedActionsDaily.Count >= limits.DailyLimits.FollowPeopleLimit:
							return new AddEventResponse
							{
								Event = events,
								ContainsErrors = false,
								HasCompleted = false,
								DailyLimitReached = true
							};
						case ActionType.FollowUser when completedActionsHourly.Count >= limits.HourlyLimits.FollowPeopleLimit:
							return new AddEventResponse
							{
								Event = events,
								ContainsErrors = false,
								HasCompleted = false,
								HourlyLimitReached = true
							};
					}
				}
				_timelineLogic.AddEventToTimeline(events.ActionName, events.Data, events.ExecutionTime);
				return new AddEventResponse
				{
					HasCompleted = true,
					Event = events
				};					
			}
			catch (Exception ee)
			{
				return new AddEventResponse
				{
					ContainsErrors = true,
					Event = events,
					Errors = new TimelineErrorResponse
					{
						Exception = ee,
						Message = ee.Message
					}
				};
			}
		}
		#region GET TIMELINE DETAILS
		private ScheduleWindow GetDayCompletedActions(string accountId, string instagramId = null, int limit = 1000)
		{
			var todays = new List<ResultBase<TimelineItem>>();
			var backwards = _timelineLogic.GetFinishedEventsForUserByDate(accountId, DateTime.UtcNow, instaId: instagramId,
				limit: limit, timelineDateType: TimelineDateType.Backwards);
			var forward = _timelineLogic.GetFinishedEventsForUserByDate(accountId, DateTime.UtcNow, instaId: instagramId,
				limit: limit, timelineDateType: TimelineDateType.Forward);

			if (backwards != null)
				todays.AddRange(backwards.Select(_ => new ResultBase<TimelineItem>
				{
					Response = new TimelineItem
					{
						ActionName = _.ActionName,
						EnqueueTime = _.SuccededAt,
						ItemId = _.ItemId,
						State = _.State,
						Url = _.Url,
						User = _.User
					},
					Message = _.Results,
					TimelineType = typeof(TimelineFinishedItem)
				}));
			if (forward != null)
				todays.AddRange(forward.Select(_ => new ResultBase<TimelineItem>
				{
					Response = new TimelineItem
					{
						ActionName = _.ActionName,
						EnqueueTime = _.SuccededAt,
						ItemId = _.ItemId,
						State = _.State,
						Url = _.Url,
						User = _.User
					},
					Message = _.Results,
					TimelineType = typeof(TimelineFinishedItem)
				}));

			if (todays.Count > 0)
			{
				var schedulerHistory = todays.OrderBy(_ => _.Response.EnqueueTime).GroupBy(_ => new { Type = typeof(TimelineFinishedItem), _.Response.ActionName });

				return new ScheduleWindow
				{
					All = schedulerHistory.Where(_ => _.Key.Type != typeof(TimelineDeletedItem) && _.Key?.ActionName?.Split('_')?[0].ToLower() != ActionType.CreatePost.GetDescription().ToLower()).SquashMe().ToList(),
					CreatePostActions = schedulerHistory.Where(_ => _.Key.Type != typeof(TimelineDeletedItem) && _.Key?.ActionName?.Split('_')?[0].ToLower() == ActionType.CreatePost.GetDescription().ToLower()).SquashMe().ToList(),
					CommentingActions = schedulerHistory.Where(_ => _.Key.Type != typeof(TimelineDeletedItem) && _.Key?.ActionName?.Split('_')?[0].ToLower() == ActionType.CreateCommentMedia.GetDescription().ToLower()).SquashMe().ToList(),
					FollowUserActions = schedulerHistory.Where(_ => _.Key.Type != typeof(TimelineDeletedItem) && _.Key?.ActionName?.Split('_')?[0].ToLower() == ActionType.FollowUser.GetDescription().ToLower()).SquashMe().ToList(),
					LikeMediaActions = schedulerHistory.Where(_ => _.Key.Type != typeof(TimelineDeletedItem) && _.Key?.ActionName?.Split('_')?[0].ToLower() == ActionType.LikePost.GetDescription().ToLower()).SquashMe().ToList()
				};
			}
			return new ScheduleWindow();
		}
		private ScheduleWindow GetHourCompletedActions(string accountId, string instagramId = null, int limit = 1000)
		{
			var todays = new List<ResultBase<TimelineItem>>();
			var backwards = _timelineLogic.GetFinishedEventsForUserByDate(accountId, DateTime.UtcNow, endDate: DateTime.UtcNow.AddHours(-1), instaId: instagramId,
				limit: limit, timelineDateType: TimelineDateType.Backwards);
			var forward = _timelineLogic.GetFinishedEventsForUserByDate(accountId, DateTime.UtcNow, endDate: DateTime.UtcNow.AddHours(-1), instaId: instagramId,
				limit: limit, timelineDateType: TimelineDateType.Forward);

			if (backwards != null)
				todays.AddRange(backwards.Select(_=>new ResultBase<TimelineItem>
				{
					Response = new TimelineItem
					{
						ActionName = _.ActionName,
						EnqueueTime = _.SuccededAt,
						ItemId = _.ItemId,
						State = _.State,
						Url = _.Url,
						User = _.User
					},
					Message = _.Results,
					TimelineType = typeof(TimelineFinishedItem)
				}));
			if (forward != null)
				todays.AddRange(forward.Select(_=>new ResultBase<TimelineItem>
				{
					Response = new TimelineItem
					{
						ActionName = _.ActionName,
						EnqueueTime = _.SuccededAt,
						ItemId = _.ItemId,
						State = _.State,
						Url = _.Url,
						User = _.User
					},
					Message = _.Results,
					TimelineType = typeof(TimelineFinishedItem)
				}));

			if (todays.Count > 0)
			{
				var schedulerHistory = todays.OrderBy(_ => _.Response.EnqueueTime).GroupBy(_ => new { Type = typeof(TimelineFinishedItem), _.Response.ActionName });

				return new ScheduleWindow
				{
					All = schedulerHistory.Where(_ => _.Key?.ActionName?.Split('_')?[0].ToLower() != ActionType.CreatePost.GetDescription().ToLower()).SquashMe().ToList(),
					CreatePostActions = schedulerHistory.Where(_ => _.Key?.ActionName?.Split('_')?[0].ToLower() == ActionType.CreatePost.GetDescription().ToLower()).SquashMe().ToList(),
					CommentingActions = schedulerHistory.Where(_ => _.Key?.ActionName?.Split('_')?[0].ToLower() == ActionType.CreateCommentMedia.GetDescription().ToLower()).SquashMe().ToList(),
					FollowUserActions = schedulerHistory.Where(_ => _.Key?.ActionName?.Split('_')?[0].ToLower() == ActionType.FollowUser.GetDescription().ToLower()).SquashMe().ToList(),
					LikeMediaActions = schedulerHistory.Where(_ => _.Key?.ActionName?.Split('_')?[0].ToLower() == ActionType.LikePost.GetDescription().ToLower()).SquashMe().ToList(),
					LikeCommentActions = schedulerHistory.Where(_ => _.Key?.ActionName?.Split('_')?[0].ToLower() == ActionType.LikeComment.GetDescription().ToLower()).SquashMe().ToList()
				};
			}
			return new ScheduleWindow();
		}
		private ScheduleWindow GetTodaysScheduleWindow (string accountId, string instagramId = null, int limit = 1000)
		{
			object _locker = new object();
			lock (_locker) { 
				var todays = new List<ResultBase<TimelineItem>>();
				var backwards = _timelineLogic.GetAllEventsForUser(accountId, DateTime.UtcNow, instaId: instagramId, 
					limit: limit,timelineDateType: TimelineDateType.Backwards);
				var forward = _timelineLogic.GetAllEventsForUser(accountId,DateTime.UtcNow,instaId:instagramId,
					limit:limit,timelineDateType:TimelineDateType.Forward);

				if(backwards!=null)
					todays.AddRange(backwards);
				if(forward!=null)
					todays.AddRange(forward);

				if (todays.Count > 0) { 
					var schedulerHistory = todays.OrderBy(_ => _.Response.EnqueueTime).GroupBy(_ => new { _.TimelineType, _.Response.ActionName });

					return new ScheduleWindow
					{
						All = schedulerHistory.Where(_ => _.Key?.ActionName?.Split('_')?[0].ToLower() != ActionType.CreatePost.GetDescription().ToLower()).SquashMe().ToList(),
						CreatePostActions = schedulerHistory.Where(_ => _.Key?.ActionName?.Split('_')?[0].ToLower() == ActionType.CreatePost.GetDescription().ToLower()).SquashMe().ToList(),
						CommentingActions = schedulerHistory.Where(_ => _.Key?.ActionName?.Split('_')?[0].ToLower() == ActionType.CreateCommentMedia.GetDescription().ToLower()).SquashMe().ToList(),
						FollowUserActions = schedulerHistory.Where(_ => _.Key?.ActionName?.Split('_')?[0].ToLower() == ActionType.FollowUser.GetDescription().ToLower()).SquashMe().ToList(),
						LikeMediaActions = schedulerHistory.Where(_ => _.Key?.ActionName?.Split('_')?[0].ToLower() == ActionType.LikePost.GetDescription().ToLower()).SquashMe().ToList(),
						LikeCommentActions = schedulerHistory.Where(_ => _.Key?.ActionName?.Split('_')?[0].ToLower() == ActionType.LikeComment.GetDescription().ToLower()).SquashMe().ToList()
					};
				}
				return null;
			}
		}
		private ScheduleWindow GetLastHoursScheduleWindow(string accountId, string instagramId = null, int limit = 1000)
		{
			var todays = new List<ResultBase<TimelineItem>>();
			var backwards = _timelineLogic.GetAllEventsForUser(accountId, DateTime.UtcNow, endDate: DateTime.UtcNow.AddHours(-1),
				instaId: instagramId, limit: limit, timelineDateType: TimelineDateType.Backwards);
			var forward = _timelineLogic.GetAllEventsForUser(accountId, DateTime.UtcNow, endDate: DateTime.UtcNow.AddHours(-1),
				instaId: instagramId, limit: limit, timelineDateType: TimelineDateType.Forward);
			
			if(backwards!=null)
				todays.AddRange(backwards);
			if(forward!=null)
				todays.AddRange(forward);
			if (todays.Count > 0) {
				var schedulerHistory = todays.OrderBy(_ => _.Response.EnqueueTime).GroupBy(_ => new { _.TimelineType, _.Response.ActionName });

				return new ScheduleWindow
				{
					All = schedulerHistory.SquashMe().ToList(),
					CreatePostActions = schedulerHistory.Where(_ => _.Key.TimelineType != typeof(TimelineDeletedItem) && _.Key?.ActionName?.Split('_')?[0].ToLower() == ActionType.CreatePost.GetDescription().ToLower()).SquashMe().ToList(),
					CommentingActions = schedulerHistory.Where(_ => _.Key.TimelineType != typeof(TimelineDeletedItem) && _.Key?.ActionName?.Split('_')?[0].ToLower() == ActionType.CreateCommentMedia.GetDescription().ToLower()).SquashMe().ToList(),
					FollowUserActions = schedulerHistory.Where(_ => _.Key.TimelineType != typeof(TimelineDeletedItem) && _.Key?.ActionName?.Split('_')?[0].ToLower() == ActionType.FollowUser.GetDescription().ToLower()).SquashMe().ToList(),
					LikeMediaActions = schedulerHistory.Where(_ => _.Key.TimelineType != typeof(TimelineDeletedItem) && _.Key?.ActionName?.Split('_')?[0].ToLower() == ActionType.LikePost.GetDescription().ToLower()).SquashMe().ToList()
				}; 
			}
			return new ScheduleWindow();
		}
		private List<ResultBase<TimelineItem>> GetEveryActionForToday (string accid, ActionType action, string instaacc = null, int limit = 5000, bool isHourly = false)
		{
			ScheduleWindow schedule = new ScheduleWindow();
			if (isHourly)
			{
				schedule = GetHourCompletedActions(accid, instaacc, limit);
			}
			else
			{
				schedule = GetDayCompletedActions(accid, instaacc, limit);
			}

			switch (action)
			{
				case ActionType.LikePost:
					return schedule.LikeMediaActions;
				case ActionType.CreateCommentMedia:
					return schedule.CommentingActions;
				case ActionType.CreatePost:
					return schedule.CreatePostActions;
				case ActionType.FollowUser:
					return schedule.FollowUserActions;
				case ActionType.LikeComment:
					return schedule.LikeCommentActions;
			}

			return null;
		}
		#endregion
		private DateTime PickAGoodTime(string accountId, string instagramAccountId, ActionType? actionName = null)
		{
			List<TimelineItem> sft;
				switch (actionName)
				{
					case null:
						sft = _timelineLogic.GetScheduledEventsForUserByDate(accountId, DateTime.UtcNow, instaId: instagramAccountId, limit: 5000, timelineDateType: TimelineDateType.Forward).ToList();
						break;
					case ActionType.CreatePost:
						sft = _timelineLogic.GetScheduledEventsForUserForActionByDate(accountId, actionName.Value.GetDescription(), DateTime.UtcNow, instaId: instagramAccountId, limit: 5000, timelineDateType: TimelineDateType.Forward).ToList();
						break;
					default:
						sft = _timelineLogic.GetScheduledEventsForUserForActionByDate(accountId, ActionType.CreateCommentMedia.GetDescription(), DateTime.UtcNow, instaId: instagramAccountId, limit: 5000, timelineDateType: TimelineDateType.Forward).ToList();
						sft.AddRange(_timelineLogic.GetScheduledEventsForUserForActionByDate(accountId, ActionType.FollowUser.GetDescription(), DateTime.UtcNow, instaId: instagramAccountId, limit: 5000, timelineDateType: TimelineDateType.Forward).ToList());
						sft.AddRange(_timelineLogic.GetScheduledEventsForUserForActionByDate(accountId, ActionType.LikePost.GetDescription(), DateTime.UtcNow, instaId: instagramAccountId, limit: 5000, timelineDateType: TimelineDateType.Forward).ToList());
						sft.AddRange(_timelineLogic.GetScheduledEventsForUserForActionByDate(accountId, ActionType.LikeComment.GetDescription(), DateTime.UtcNow, instaId: instagramAccountId, limit: 5000, timelineDateType: TimelineDateType.Forward).ToList());
						sft.AddRange(_timelineLogic.GetScheduledEventsForUserForActionByDate(accountId, ActionType.UnFollowUser.GetDescription(), DateTime.UtcNow, instaId: instagramAccountId, limit: 5000, timelineDateType: TimelineDateType.Forward).ToList());
						break;
				}
				var datesPlanned = sft.Select(_ => _.EnqueueTime);
				var dateTimes = datesPlanned as DateTime?[] ?? datesPlanned.ToArray();
				if (!dateTimes.Any()) return DateTime.UtcNow;
				{
					var current = DateTime.UtcNow;
					var difference = dateTimes.Where(_ => _ != null).Max(_ => _ - current);
					var position = dateTimes.ToList().FindIndex(n => n - current == difference);
					return dateTimes.ElementAt(position).Value;
				}

		}
		public async Task Begin()
		{
			//TODO : MAKE SURE ALL USERS ARE BUSINESS ACCOUNTS AND USERS OVER 100 FOLLOWERS BASE THEIR POSTING ON WHICH HOUR WAS MOST POPULAR
			var numberOfWorkers = 0;
			while (true) {
				if (numberOfWorkers < 5) { 
					_ = Task.Run(async () => { 
						var allActives = await _agentLogic.GetAllAccounts();
						var shortInstagramAccountModels = allActives as ShortInstagramAccountModel[] ?? allActives.ToArray();
						var instaAccount = shortInstagramAccountModels?.Shuffle().ElementAtOrDefault(SecureRandom.Next(shortInstagramAccountModels.Count()-1));
						if (instaAccount != null) {
							var _userStoreDetails = new UserStoreDetails();
							try 
							{
								#region Token Stuff
							
								var acc = (await _authHandler.GetUserByUsername(instaAccount.AccountId));
								var expTime = acc.Claims?.Where(s=>s.Type == "exp")?.SingleOrDefault();
								var accessToken = acc.Tokens.SingleOrDefault(_ => _.Name == "access_token")?.Value;
								var refreshToken = acc.Tokens.SingleOrDefault(_ => _.Name == "refresh_token")?.Value;
								var idToken = acc.Tokens.SingleOrDefault(_ => _.Name == "id_token")?.Value;
								if (instaAccount.UserLimits == null)
								{
									instaAccount.UserLimits = SetLimits(acc.Roles.FirstOrDefault().GetValueFromDescription<AuthTypes>());
								}
								_userStoreDetails.ORefreshToken = refreshToken;
								if (expTime != null)
								{
									var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
									var toda = epoch.AddSeconds(long.Parse(expTime.Value));
									if(DateTime.UtcNow > toda.AddMinutes(-20))
									{
										var res = await _authHandler.RefreshLogin(refreshToken, acc.UserName);
										instaAccount.UserLimits = SetLimits(acc.Roles.FirstOrDefault().GetValueFromDescription<AuthTypes>());
										await _instagramAccountLogic.PartialUpdateInstagramAccount(instaAccount.AccountId,instaAccount.Id, new InstagramAccountModel{UserLimits = instaAccount.UserLimits});
										accessToken = res.Results.AuthenticationResult.AccessToken;
										idToken = res.Results.AuthenticationResult.IdToken;
									
										_userStoreDetails.AddUpdateUser(instaAccount.AccountId, instaAccount.Id, idToken);
										_userStoreDetails.OInstagramAccountUsername = instaAccount.Username;
										var items = _timelineLogic.GetScheduledEventsForUser(_userStoreDetails.OAccountId, _userStoreDetails.OInstagramAccountUser,1000).ToList();
									
										items.ForEach(_ =>
										{
											_timelineLogic.DeleteEvent(_.ItemId);
											_.User = _userStoreDetails;
											_.Rest.User = _userStoreDetails;
											_timelineLogic.AddEventToTimeline(_.ActionName, _.Rest, _.EnqueueTime.Value.AddSeconds(15));
										});
									}
									else
									{
										_userStoreDetails.AddUpdateUser(instaAccount.AccountId, instaAccount.Id, idToken);
										_userStoreDetails.OInstagramAccountUsername = instaAccount.Username;							
									}
								}
								#endregion
								var profile = await _profileLogic.GetProfile(_userStoreDetails.OAccountId, _userStoreDetails.OInstagramAccountUser);
								if (profile == null) return;
								_userStoreDetails.shortInstagram = instaAccount;
								_userStoreDetails.Profile = profile;
								var nextAvaliableDate = PickAGoodTime(_userStoreDetails.OAccountId, _userStoreDetails.OInstagramAccountUser);
								#region Unfollow Section
								if (instaAccount.LastPurgeCycle == null)
								{
									await _instagramAccountLogic.PartialUpdateInstagramAccount(_userStoreDetails.OAccountId, _userStoreDetails.OInstagramAccountUser,
										new InstagramAccountModel
										{
											LastPurgeCycle = DateTime.UtcNow.AddHours(33)
										});
								}			
								else
								{
									if(DateTime.UtcNow > instaAccount.LastPurgeCycle)
									{
										if (instaAccount.FollowingCount != null)
										{
											var res = ActionsManager.Begin.Commit(ActionType.UnFollowUser, _contentManager, _heartbeatLogic)
												.IncludeStrategy(new UnFollowStrategySettings
												{
													UnFollowStrategy = UnFollowStrategyType.LeastEngagingN,
													NumberOfUnfollows = (int)(instaAccount.FollowingCount.Value * 0.65),
													OffsetPerAction = TimeSpan.FromSeconds(20)
												})
												.IncludeUser(_userStoreDetails)
												.Push(new UnfollowActionOptions(nextAvaliableDate.AddSeconds(UnfollowActionOptions.TimeFrameSeconds.Max)));

											if (res.IsSuccesful)
											{
												foreach (var r in res.Results)
													_timelineLogic.AddEventToTimeline(r.ActionName, r.Data, r.ExecutionTime);

												await _instagramAccountLogic.PartialUpdateInstagramAccount(_userStoreDetails.OAccountId, _userStoreDetails.OInstagramAccountUser,
													new InstagramAccountModel
													{
														LastPurgeCycle = DateTime.UtcNow.AddHours(6)
													});
											}
										}
									}
								}
								#endregion
								if (instaAccount.DateAdded.HasValue)
								{
									SetLimits(instaAccount.DateAdded.Value);
								}						
								var totalforuser = GetTodaysScheduleWindow(_userStoreDetails.OAccountId, _userStoreDetails.OInstagramAccountUser)?.All?.ToList();			
								#region Action Initialise
								var actionsContainerManager = new ActionsContainerManager();

								var likeAction = ActionsManager.Begin.Commit(ActionType.LikePost, _contentManager, _heartbeatLogic)
									.IncludeStrategy(new LikeStrategySettings()
									{
										LikeStrategy = LikeStrategyType.Default,
									})
									.IncludeUser(_userStoreDetails);
								var postAction = ActionsManager.Begin.Commit(ActionType.CreatePost, _contentManager, _heartbeatLogic)
									.IncludeStrategy(new ImageStrategySettings
									{
										ImageStrategyType = ImageStrategyType.Default
									})
									.IncludeUser(_userStoreDetails);
								var followAction = ActionsManager.Begin.Commit(ActionType.FollowUser, _contentManager, _heartbeatLogic)
									.IncludeStrategy(new FollowStrategySettings
									{
										FollowStrategy = FollowStrategyType.Default,
									})
									.IncludeUser(_userStoreDetails);
								var commentAction = ActionsManager.Begin.Commit(ActionType.CreateCommentMedia, _contentManager, _heartbeatLogic)
									.IncludeStrategy(new CommentingStrategySettings
									{
										CommentingStrategy = CommentingStrategy.Default,
									})
									.IncludeUser(_userStoreDetails);
								var likeCommentAction = ActionsManager.Begin.Commit(ActionType.LikeComment, _contentManager, _heartbeatLogic)
									.IncludeStrategy(new LikeStrategySettings())
									.IncludeUser(_userStoreDetails);

								//Initial Execution
								var likeScheduleOptions = new LikeActionOptions(nextAvaliableDate.AddMinutes(SecureRandom.Next(1, 4)), LikeActionType.Any);
								var postScheduleOptions = new PostActionOptions(nextAvaliableDate.AddMinutes(SecureRandom.Next(1, 5))) { ImageFetchLimit = 20 };
								var followScheduleOptions = new FollowActionOptions(nextAvaliableDate.AddMinutes(SecureRandom.Next(1, 4)), FollowActionType.Any);
								var commentScheduleOptions = new CommentingActionOptions(nextAvaliableDate.AddMinutes(SecureRandom.Next(1, 4)), CommentingActionType.Any);
								var likecommentScheduleOptions = new LikeCommentActionOptions(nextAvaliableDate.AddMinutes(SecureRandom.Next(4)), LikeCommentActionType.Any);
				
								actionsContainerManager.AddAction(postAction, postScheduleOptions, 0.10);
								actionsContainerManager.AddAction(likeAction, likeScheduleOptions, 0.275);
								actionsContainerManager.AddAction(followAction, followScheduleOptions, 0.20);
								actionsContainerManager.AddAction(commentAction, commentScheduleOptions, 0.15);
								actionsContainerManager.AddAction(likeCommentAction, likecommentScheduleOptions, 0.275);
								#endregion
								#region Agent State
								if (instaAccount == null) return;
								if(instaAccount.AgentState == (int) AgentState.NotStarted)
								{
									instaAccount.AgentState = (int) AgentState.Running;
								}
								switch (instaAccount.AgentState)
								{
									case (int) AgentState.Running:
									{
										if (totalforuser != null) { 
											if (totalforuser.Count > 100)
											{
												instaAccount.AgentState = (int) AgentState.Sleeping;
											} 
										}
										var nominatedAction = actionsContainerManager.GetRandomAction();
										actionsContainerManager.AddWork(nominatedAction);
										actionsContainerManager.RunAction();
										var finishedAction = actionsContainerManager.GetFinishedActions()?.DistinctBy(d => d.Data);
										finishedAction?.ForEach(_ =>
										{
											string actionName = _.ActionName.Split('_')[0].ToLower();
											var atype = actionName.GetValueFromDescription<ActionType>();
											var timeSett = actionsContainerManager.FindActionLimit(atype);

											nextAvaliableDate = PickAGoodTime(_userStoreDetails.OAccountId, _userStoreDetails.OInstagramAccountUser, actionName.GetValueFromDescription<ActionType>());
											actionsContainerManager.HasMetTimeLimit();
											if (nextAvaliableDate == null) return;
											_.ExecutionTime = nextAvaliableDate.AddSeconds(timeSett.Value.Max);
											var res_ = AddToTimeline(_,instaAccount.UserLimits);
											if (res_.HasCompleted)
											{

											}
											if (res_.DailyLimitReached)
											{
												actionsContainerManager.TriggerAction(actionName.GetValueFromDescription<ActionType>(), DateTime.UtcNow.AddDays(1));
											}
											else if (res_.HourlyLimitReached)
											{
												actionsContainerManager.TriggerAction(actionName.GetValueFromDescription<ActionType>(), DateTime.UtcNow.AddHours(1));
											}
										});

										break;
									}
									case (int) AgentState.Sleeping when totalforuser == null:
										instaAccount.AgentState = (int) AgentState.Running;
										break;
									case (int) AgentState.Sleeping:
									{
										if(totalforuser.Count <= 10)
											instaAccount.AgentState = (int)AgentState.Running;
										break;
									}
									case (int) AgentState.Blocked:
									{
										var items = _timelineLogic.GetScheduledEventsForUser(_userStoreDetails.OAccountId, _userStoreDetails.OInstagramAccountUser, 2000).ToList();
										items.ForEach(_ =>
										{
											_timelineLogic.DeleteEvent(_.ItemId);
										});
										instaAccount.SleepTimeRemaining = DateTime.UtcNow.AddHours(24);
										instaAccount.AgentState = (int) AgentState.DeepSleep;
										await _instagramAccountLogic.PartialUpdateInstagramAccount(_userStoreDetails.OAccountId, _userStoreDetails.OInstagramAccountUser, new InstagramAccountModel
										{
											SleepTimeRemaining = instaAccount.SleepTimeRemaining
										});
										break;
									}
									case (int) AgentState.Challenge:
									{
										var items = _timelineLogic.GetScheduledEventsForUser(_userStoreDetails.OAccountId, _userStoreDetails.OInstagramAccountUser, 2000).ToList();
										items.ForEach(_ =>
										{
											_timelineLogic.DeleteEvent(_.ItemId);
										});
										instaAccount.AgentState = (int)AgentState.AwaitingActionFromUser;
										break;
									}
									case (int)AgentState.DeepSleep when instaAccount.SleepTimeRemaining.HasValue:
									{
										if(DateTime.UtcNow > instaAccount.SleepTimeRemaining.Value)
										{
											instaAccount.AgentState = (int) AgentState.Running;
										}

										break;
									}
									case (int)AgentState.DeepSleep:
										instaAccount.AgentState = (int) AgentState.Running;
										break;
									case (int) AgentState.Stopped:
										break;
								}
								await _instagramAccountLogic.PartialUpdateInstagramAccount(_userStoreDetails.OAccountId, _userStoreDetails.OInstagramAccountUser, new InstagramAccountModel
								{
									AgentState = instaAccount.AgentState,
								});
								#endregion
							}
							catch (Exception ee)
							{
								Console.WriteLine(ee.Message);
							}
						}
					}).ContinueWith(x=>numberOfWorkers--);
					numberOfWorkers++;
				}
				await Task.Delay(TimeSpan.FromSeconds(1));
			}
		}
		private Limits SetLimits(AuthTypes authType) 
		{
			// hardcoded values for now, probably will take from app variable
			switch (authType)
			{
				case AuthTypes.Admin:
					return new Limits
					{
						DailyLimits = new DailyActions
						{
							CreateCommentLimit = 500,
							CreatePostLimit = 24,
							FollowPeopleLimit = 225,
							FollowTopicLimit = 225,
							LikeCommentLimit = 900,
							LikePostLimit = 900
						},
						HourlyLimits = new HourlyActions
						{
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
							CreateCommentLimit = 100,
							CreatePostLimit = 8,
							FollowPeopleLimit = 80,
							FollowTopicLimit = 80,
							LikeCommentLimit = 200,
							LikePostLimit = 200
						},
						HourlyLimits = new HourlyActions
						{
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
							CreateCommentLimit = 499,
							CreatePostLimit = 23,
							FollowPeopleLimit = 220,
							FollowTopicLimit = 220,
							LikeCommentLimit = 899,
							LikePostLimit = 899
						},
						HourlyLimits = new HourlyActions
						{
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
				case AuthTypes.PremiumUsers:
					return new Limits
					{
						DailyLimits = new DailyActions
						{
							CreateCommentLimit = 280,
							CreatePostLimit = 15,
							FollowPeopleLimit = 125,
							FollowTopicLimit = 125,
							LikeCommentLimit = 600,
							LikePostLimit = 600
						},
						HourlyLimits = new HourlyActions
						{
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
							CreateCommentLimit = 60,
							CreatePostLimit = 4,
							FollowPeopleLimit = 40,
							FollowTopicLimit = 40,
							LikeCommentLimit = 100,
							LikePostLimit = 100
						},
						HourlyLimits = new HourlyActions
						{
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
		private void SetLimits(DateTime date)
		{
			if (DateTime.UtcNow.Subtract(date).TotalDays < 7)
			{
				LikeActionOptions.LikeActionDailyLimit = new Range(400, 500);
				LikeActionOptions.LikeActionHourlyLimit = new Range(20, 30);

				CommentingActionOptions.CommentingActionDailyLimit = new Range(150, 250);
				CommentingActionOptions.CommentingActionHourlyLimit = new Range(15, 30);

				LikeCommentActionOptions.LikeActionDailyLimit = new Range(400, 500);
				LikeCommentActionOptions.LikeActionHourlyLimit = new Range(20, 30);

				PostActionOptions.CreatePostActionDailyLimit = new Range(8, 10);
				PostActionOptions.CreatePostActionHourlyLimit = new Range(1, 2);
			}
		}
	}
}
