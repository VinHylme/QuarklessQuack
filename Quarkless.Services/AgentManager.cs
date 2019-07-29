using Quarkless.Services.ActionBuilders.EngageActions;
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
	public class AgentManager : IAgentManager
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

		private AddEventResponse AddToTimeline(TimelineEventModel events)
		{
			if (events != null) {
				try
					{
					var actionBase = events.ActionName.Split('_')[0].ToLower().GetValueFromDescription<ActionType>();
					List<ResultBase<TimelineItem>> completedActionsHourly = GetEveryActionForToday(events.Data.User.OAccountId,actionBase,events.Data.User.OInstagramAccountUser,isHourly:true);
					List<ResultBase<TimelineItem>> completedActionsDaily = GetEveryActionForToday(events.Data.User.OAccountId, actionBase, events.Data.User.OInstagramAccountUser);
					if(completedActionsDaily!=null && completedActionsHourly != null) { 
						if (actionBase==(ActionType.CreatePost))
						{
							if (completedActionsDaily.Count >= PostActionOptions.CreatePostActionDailyLimit.Max)
							{
								return new AddEventResponse
								{
									Event = events,
									ContainsErrors = false,
									HasCompleted = false,
									DailyLimitReached = true
								};
							}
						}
						else if (actionBase==(ActionType.CreateCommentMedia))
						{
							if (completedActionsDaily.Count >= CommentingActionOptions.CommentingActionDailyLimit.Max)
							{
								return new AddEventResponse
								{
									Event = events,
									ContainsErrors = false,
									HasCompleted = false,
									DailyLimitReached = true
								};
							}
							if (completedActionsHourly.Count >= CommentingActionOptions.CommentingActionHourlyLimit.Max)
							{
								return new AddEventResponse
								{
									Event = events,
									ContainsErrors = false,
									HasCompleted = false,
									HourlyLimitReached = true
								};
							}
						}
						else if (actionBase==(ActionType.LikePost))
						{
							if (completedActionsDaily.Count >= LikeActionOptions.LikeActionDailyLimit.Max)
							{
								return new AddEventResponse
								{
									Event = events,
									ContainsErrors = false,
									HasCompleted = false,
									DailyLimitReached = true
								};
							}
							if (completedActionsHourly.Count >= LikeActionOptions.LikeActionHourlyLimit.Max)
							{
								new AddEventResponse
								{
									Event = events,
									ContainsErrors = false,
									HasCompleted = false,
									HourlyLimitReached = true
								};
							}
						}
						else if(actionBase == (ActionType.LikeComment))
						{
							if(completedActionsDaily.Count >= LikeCommentActionOptions.LikeActionDailyLimit.Max)
							{
								return new AddEventResponse
								{
									Event = events,
									ContainsErrors = false,
									HasCompleted = false,
									DailyLimitReached = true
								};
							}
							if (completedActionsHourly.Count >= LikeCommentActionOptions.LikeActionHourlyLimit.Max)
							{
								return new AddEventResponse
								{
									Event = events,
									ContainsErrors = false,
									HasCompleted = false,
									HourlyLimitReached = true
								};
							}
						}
						else if (actionBase==(ActionType.FollowUser))
					{
						if (completedActionsDaily.Count >= FollowActionOptions.FollowActionDailyLimit.Max)
						{
							return new AddEventResponse
							{
								Event = events,
								ContainsErrors = false,
								HasCompleted = false,
								DailyLimitReached = true
							};
						}
						if (completedActionsHourly.Count >= FollowActionOptions.FollowActionHourlyLimit.Max)
						{
							return new AddEventResponse
							{
								Event = events,
								ContainsErrors = false,
								HasCompleted = false,
								HourlyLimitReached = true
							};
						}
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
			return null;
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
				if (actionName == null)
				{

					sft = _timelineLogic.GetScheduledEventsForUserByDate(accountId, DateTime.UtcNow, instaId: instagramAccountId, limit: 5000, timelineDateType: TimelineDateType.Forward).ToList();
				}
				else
				{
					if (actionName == ActionType.CreatePost)
					{
						sft = _timelineLogic.GetScheduledEventsForUserForActionByDate(accountId, actionName.Value.GetDescription(), DateTime.UtcNow, instaId: instagramAccountId, limit: 5000, timelineDateType: TimelineDateType.Forward).ToList();
					}
					else
					{
						sft = _timelineLogic.GetScheduledEventsForUserForActionByDate(accountId, ActionType.CreateCommentMedia.GetDescription(), DateTime.UtcNow, instaId: instagramAccountId, limit: 5000, timelineDateType: TimelineDateType.Forward).ToList();
						sft.AddRange(_timelineLogic.GetScheduledEventsForUserForActionByDate(accountId, ActionType.FollowUser.GetDescription(), DateTime.UtcNow, instaId: instagramAccountId, limit: 5000, timelineDateType: TimelineDateType.Forward).ToList());
						sft.AddRange(_timelineLogic.GetScheduledEventsForUserForActionByDate(accountId, ActionType.LikePost.GetDescription(), DateTime.UtcNow, instaId: instagramAccountId, limit: 5000, timelineDateType: TimelineDateType.Forward).ToList());
						sft.AddRange(_timelineLogic.GetScheduledEventsForUserForActionByDate(accountId, ActionType.LikeComment.GetDescription(), DateTime.UtcNow, instaId: instagramAccountId, limit: 5000, timelineDateType: TimelineDateType.Forward).ToList());
						sft.AddRange(_timelineLogic.GetScheduledEventsForUserForActionByDate(accountId, ActionType.UnFollowUser.GetDescription(), DateTime.UtcNow, instaId: instagramAccountId, limit: 5000, timelineDateType: TimelineDateType.Forward).ToList());
					}
				}
				var datesPlanned = sft.Select(_ => _.EnqueueTime);
				if (datesPlanned != null && datesPlanned.Count() > 0)
				{
					DateTime current = DateTime.UtcNow;
					var difference = datesPlanned.Where(_ => _ != null).Max(_ => _ - current);
					var position = datesPlanned.ToList().FindIndex(n => n - current == difference);
					return datesPlanned.ElementAt(position).Value;
				}
				else
				{
					return DateTime.UtcNow;
				}
			}
		public void Begin()
		{
			//TODO : MAKE SURE ALL USERS ARE BUSINESS ACCOUNTS AND USERS OVER 100 FOLLOWERS BASE THEIR POSTING ON WHICH HOUR WAS MOST POPULAR
			int numberOfWorkers = 0;
			while (true) {
				if (numberOfWorkers < 10) { 
					_=Task.Run(async () => { 
						IEnumerable<ShortInstagramAccountModel> allActives = await _agentLogic.GetAllAccounts();
						if (allActives != null) { 
						var _instaAccount = allActives.Shuffle().ElementAtOrDefault(SecureRandom.Next(allActives.Count()-1));
						if (_instaAccount != null) {
							UserStoreDetails _userStoreDetails = new UserStoreDetails();
							try 
							{
								#region Token Stuff
							
								var acc = (await _authHandler.GetUserByUsername(_instaAccount.AccountId));
								var expTime = acc.Claims?.Where(s=>s.Type == "exp")?.SingleOrDefault();
								var accessToken = acc.Tokens.Where(_ => _.Name == "access_token").SingleOrDefault()?.Value;
								var refreshToken = acc.Tokens.Where(_ => _.Name == "refresh_token").SingleOrDefault()?.Value;
								var idToken = acc.Tokens.Where(_ => _.Name == "id_token").SingleOrDefault()?.Value;

								_userStoreDetails.ORefreshToken = refreshToken;
								if (expTime != null)
								{
									var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
									var toda = epoch.AddSeconds(long.Parse(expTime.Value));
									if(DateTime.UtcNow > toda.AddMinutes(-20))
									{
										var res = await _authHandler.RefreshLogin(refreshToken, acc.UserName);

										accessToken = res.Results.AuthenticationResult.AccessToken;
										idToken = res.Results.AuthenticationResult.IdToken;
									
										_userStoreDetails.AddUpdateUser(_instaAccount.AccountId, _instaAccount.Id, idToken);
										_userStoreDetails.OInstagramAccountUsername = _instaAccount.Username;
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
										_userStoreDetails.AddUpdateUser(_instaAccount.AccountId, _instaAccount.Id, idToken);
										_userStoreDetails.OInstagramAccountUsername = _instaAccount.Username;							
									}
								}
								#endregion
								var profile = await _profileLogic.GetProfile(_userStoreDetails.OAccountId, _userStoreDetails.OInstagramAccountUser);
								if (profile == null) return;
								DateTime nextAvaliableDate = PickAGoodTime(_userStoreDetails.OAccountId, _userStoreDetails.OInstagramAccountUser);
								#region Unfollow Section
								if (_instaAccount.LastPurgeCycle == null)
								{
									await _instagramAccountLogic.PartialUpdateInstagramAccount(_userStoreDetails.OAccountId, _userStoreDetails.OInstagramAccountUser,
										new InstagramAccountModel
										{
											LastPurgeCycle = DateTime.UtcNow.AddHours(6)
										});
								}			
								else
								{
									if(DateTime.UtcNow > _instaAccount.LastPurgeCycle)
									{
										var res = ActionsManager.Begin.Commit(ActionType.UnFollowUser, _contentManager, _heartbeatLogic, profile)
											.IncludeStrategy(new UnFollowStrategySettings
												{
													UnFollowStrategy = UnFollowStrategyType.LeastEngagingN,
													NumberOfUnfollows = (int)(_instaAccount.FollowingCount.Value * 0.65),
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
								#endregion
								if (_instaAccount.DateAdded.HasValue)
								{
									SetLimits(_instaAccount.DateAdded.Value);
								}						
								var totalforuser = GetTodaysScheduleWindow(_userStoreDetails.OAccountId, _userStoreDetails.OInstagramAccountUser)?.All?.ToList();			
								#region Action Initialise
								ActionsContainerManager actionsContainerManager = new ActionsContainerManager();

								var likeAction = ActionsManager.Begin.Commit(ActionType.LikePost, _contentManager, _heartbeatLogic, profile)
									.IncludeStrategy(new LikeStrategySettings()
									{
										LikeStrategy = LikeStrategyType.Default,
									})
									.IncludeUser(_userStoreDetails);
								var postAction = ActionsManager.Begin.Commit(ActionType.CreatePost, _contentManager, _heartbeatLogic, profile)
									.IncludeStrategy(new ImageStrategySettings
									{
										ImageStrategyType = ImageStrategyType.Default
									})
									.IncludeUser(_userStoreDetails);
								var followAction = ActionsManager.Begin.Commit(ActionType.FollowUser, _contentManager, _heartbeatLogic, profile)
									.IncludeStrategy(new FollowStrategySettings
									{
										FollowStrategy = FollowStrategyType.Default,
									})
									.IncludeUser(_userStoreDetails);
								var commentAction = ActionsManager.Begin.Commit(ActionType.CreateCommentMedia, _contentManager, _heartbeatLogic, profile)
									.IncludeStrategy(new CommentingStrategySettings
									{
										CommentingStrategy = CommentingStrategy.Default,
									})
									.IncludeUser(_userStoreDetails);
								var likeCommentAction = ActionsManager.Begin.Commit(ActionType.LikeComment, _contentManager, _heartbeatLogic, profile)
									.IncludeStrategy(new LikeStrategySettings())
									.IncludeUser(_userStoreDetails);

								//Initial Execution
								var likeScheduleOptions = new LikeActionOptions(nextAvaliableDate.AddMinutes(SecureRandom.Next(1, 4)), LikeActionType.Any);
								var postScheduleOptions = new PostActionOptions(nextAvaliableDate.AddMinutes(SecureRandom.Next(1, 5))) { ImageFetchLimit = 20 };
								var followScheduleOptions = new FollowActionOptions(nextAvaliableDate.AddMinutes(SecureRandom.Next(1, 4)), FollowActionType.Any);
								var commentScheduleOptions = new CommentingActionOptions(nextAvaliableDate.AddMinutes(SecureRandom.Next(1, 4)), CommentingActionType.Any);
								var likecommentScheduleOptions = new LikeCommentActionOptions(nextAvaliableDate.AddMinutes(SecureRandom.Next(4)), LikeCommentActionType.Any);
				
								actionsContainerManager.AddAction(postAction, postScheduleOptions, 0.10);
								actionsContainerManager.AddAction(likeAction, likeScheduleOptions, 0.35);
								actionsContainerManager.AddAction(followAction, followScheduleOptions, 0.15);
								actionsContainerManager.AddAction(commentAction, commentScheduleOptions, 0.19);
								actionsContainerManager.AddAction(likeCommentAction, likecommentScheduleOptions, 0.21);
								#endregion
								#region Agent State
								if (_instaAccount == null) return;
								if(_instaAccount.AgentState == (int) AgentState.NotStarted)
								{
									_instaAccount.AgentState = (int) AgentState.Running;
								}
								if (_instaAccount.AgentState == (int) AgentState.Running)
								{
									if (totalforuser != null) { 
										if (totalforuser.Count > 500)
										{
											_instaAccount.AgentState = (int) AgentState.Sleeping;
										} 
									}
									var nominatedAction = actionsContainerManager.GetRandomAction();
									actionsContainerManager.AddWork(nominatedAction);
									actionsContainerManager.RunAction();
									var finishedAction = actionsContainerManager.GetFinishedActions()?.DistinctBy(d => d.Data);
									if (finishedAction != null)
									{
										finishedAction.ForEach(_ =>
										{
											string actionName = _.ActionName.Split('_')[0].ToLower();
											var atype = actionName.GetValueFromDescription<ActionType>();
											var timeSett = actionsContainerManager.FindActionLimit(atype);

											nextAvaliableDate = PickAGoodTime(_userStoreDetails.OAccountId, _userStoreDetails.OInstagramAccountUser, actionName.GetValueFromDescription<ActionType>());
											actionsContainerManager.HasMetTimeLimit();
											if (nextAvaliableDate != null)
											{
												_.ExecutionTime = nextAvaliableDate.AddSeconds(timeSett.Value.Max);
												var res_ = AddToTimeline(_);
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
											}
										});
									}
								}
								else if (_instaAccount.AgentState == (int) AgentState.Sleeping)
								{
									if (totalforuser == null)
										_instaAccount.AgentState = (int) AgentState.Running;
									else if(totalforuser.Count <= 10)
										_instaAccount.AgentState = (int)AgentState.Running;

								}
								else if(_instaAccount.AgentState == (int) AgentState.Blocked)
								{
									var items = _timelineLogic.GetScheduledEventsForUser(_userStoreDetails.OAccountId, _userStoreDetails.OInstagramAccountUser, 2000).ToList();
									items.ForEach(_ =>
									{
										_timelineLogic.DeleteEvent(_.ItemId);
									});
									_instaAccount.SleepTimeRemaining = DateTime.UtcNow.AddHours(24);
									_instaAccount.AgentState = (int) AgentState.DeepSleep;
									await _instagramAccountLogic.PartialUpdateInstagramAccount(_userStoreDetails.OAccountId, _userStoreDetails.OInstagramAccountUser, new InstagramAccountModel
									{
										SleepTimeRemaining = _instaAccount.SleepTimeRemaining
									});
								}
								else if(_instaAccount.AgentState == (int) AgentState.Challenge)
								{
									var items = _timelineLogic.GetScheduledEventsForUser(_userStoreDetails.OAccountId, _userStoreDetails.OInstagramAccountUser, 2000).ToList();
									items.ForEach(_ =>
									{
										_timelineLogic.DeleteEvent(_.ItemId);
									});
									_instaAccount.AgentState = (int)AgentState.AwaitingActionFromUser;
								}
								else if(_instaAccount.AgentState == (int)AgentState.DeepSleep)
								{
									if (_instaAccount.SleepTimeRemaining.HasValue)
									{
										if(DateTime.UtcNow > _instaAccount.SleepTimeRemaining.Value)
										{
											_instaAccount.AgentState = (int) AgentState.Running;
										}
									}
									else
									{
										_instaAccount.AgentState = (int) AgentState.Running;
									}
								}
								else if (_instaAccount.AgentState == (int) AgentState.Stopped)
								{

								}
								await _instagramAccountLogic.PartialUpdateInstagramAccount(_userStoreDetails.OAccountId, _userStoreDetails.OInstagramAccountUser, new InstagramAccountModel
								{
									AgentState = _instaAccount.AgentState,
								});
								#endregion
							}
							catch (Exception ee)
							{
								Console.WriteLine(ee.Message);
							}
						}
					}
					}).ContinueWith(x=>numberOfWorkers--);
					numberOfWorkers++;
				}
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
