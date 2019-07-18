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
using System.Net;
using System.Threading.Tasks;
using System.Linq;
using Quarkless.Services.Extensions;
using QuarklessLogic.Logic.InstagramAccountLogic;
using System.Timers;
using QuarklessLogic.Logic.AuthLogic.Auth;
using QuarklessContexts.Models.InstagramAccounts;
using QuarklessLogic.Logic.ProfileLogic;
using QuarklessLogic.ServicesLogic.HeartbeatLogic;
using MoreLinq;
using QuarklessContexts.Models.AgentModels;

namespace Quarkless.Services
{
	class ScheduleWindow
	{
		public IEnumerable<ResultBase<TimelineItem>> CreatePostActions;
		public IEnumerable<ResultBase<TimelineItem>> CommentingActions;
		public IEnumerable<ResultBase<TimelineItem>> LikeMediaActions;
		public IEnumerable<ResultBase<TimelineItem>> LikeCommentActions;
		public IEnumerable<ResultBase<TimelineItem>> FollowUserActions;
		public IEnumerable<ResultBase<TimelineItem>> All;
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
	public class AgentManager : IAgentManager
	{
		private readonly IInstagramAccountLogic _instagramAccountLogic;
		private readonly IProfileLogic _profileLogic;
		private readonly IContentManager _contentManager;
		private readonly ITimelineLogic _timelineLogic;
		private readonly IAuthHandler _authHandler;
		private readonly IHeartbeatLogic _heartbeatLogic;
	//	private ActionStates CurrentStates;
		public AgentManager(IInstagramAccountLogic instagramAccountLogic, IProfileLogic profileLogic ,IContentManager contentManager,
			ITimelineLogic timelineLogic,IHeartbeatLogic heartbeatLogic, IAuthHandler authHandler)
		{
			_heartbeatLogic = heartbeatLogic;
			_timelineLogic = timelineLogic;
			_instagramAccountLogic = instagramAccountLogic;
			_profileLogic = profileLogic;
			_contentManager = contentManager;
			_authHandler = authHandler;
		//	CurrentStates = new ActionStates();
		}
		private AddEventResponse AddToTimeline(TimelineEventModel events)
		{
			if (events != null) {
				try
					{
					var actionBase = events.ActionName.Split('_')[0].ToLower().GetValueFromDescription<ActionType>();
					var completedActionsHourly = GetEveryActionForToday(events.Data.User.OAccountId,actionBase,events.Data.User.OInstagramAccountUser,isHourly:true);
					var completedActionsDaily = GetEveryActionForToday(events.Data.User.OAccountId, actionBase, events.Data.User.OInstagramAccountUser);

					if (actionBase==(ActionType.CreatePost))
					{
						if (completedActionsDaily.Count() >= PostActionOptions.CreatePostActionDailyLimit.Max)
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
						if (completedActionsDaily.Count() >= CommentingActionOptions.CommentingActionDailyLimit.Max)
						{
							return new AddEventResponse
							{
								Event = events,
								ContainsErrors = false,
								HasCompleted = false,
								DailyLimitReached = true
							};
						}
						if (completedActionsHourly.Count() >= CommentingActionOptions.CommentingActionHourlyLimit.Max)
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
						if (completedActionsDaily.Count() >= LikeActionOptions.LikeActionDailyLimit.Max)
						{
							return new AddEventResponse
							{
								Event = events,
								ContainsErrors = false,
								HasCompleted = false,
								DailyLimitReached = true
							};
						}
						if (completedActionsHourly.Count() >= LikeActionOptions.LikeActionHourlyLimit.Max)
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
					else if (actionBase==(ActionType.FollowUser))
					{
						if (completedActionsDaily.Count() >= FollowActionOptions.FollowActionDailyLimit.Max)
						{
							return new AddEventResponse
							{
								Event = events,
								ContainsErrors = false,
								HasCompleted = false,
								DailyLimitReached = true
							};
						}
						if (completedActionsHourly.Count() >= FollowActionOptions.FollowActionHourlyLimit.Max)
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
					All = schedulerHistory.Where(_ => _.Key.Type != typeof(TimelineDeletedItem) && _.Key?.ActionName?.Split('_')?[0].ToLower() != ActionType.CreatePost.GetDescription().ToLower()).SquashMe(),
					CreatePostActions = schedulerHistory.Where(_ => _.Key.Type != typeof(TimelineDeletedItem) && _.Key?.ActionName?.Split('_')?[0].ToLower() == ActionType.CreatePost.GetDescription().ToLower()).SquashMe(),
					CommentingActions = schedulerHistory.Where(_ => _.Key.Type != typeof(TimelineDeletedItem) && _.Key?.ActionName?.Split('_')?[0].ToLower() == ActionType.CreateCommentMedia.GetDescription().ToLower()).SquashMe(),
					FollowUserActions = schedulerHistory.Where(_ => _.Key.Type != typeof(TimelineDeletedItem) && _.Key?.ActionName?.Split('_')?[0].ToLower() == ActionType.FollowUser.GetDescription().ToLower()).SquashMe(),
					LikeMediaActions = schedulerHistory.Where(_ => _.Key.Type != typeof(TimelineDeletedItem) && _.Key?.ActionName?.Split('_')?[0].ToLower() == ActionType.LikePost.GetDescription().ToLower()).SquashMe()
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
					All = schedulerHistory.Where(_ => _.Key.Type != typeof(TimelineDeletedItem) && _.Key?.ActionName?.Split('_')?[0].ToLower() != ActionType.CreatePost.GetDescription().ToLower()).SquashMe(),
					CreatePostActions = schedulerHistory.Where(_ => _.Key.Type != typeof(TimelineDeletedItem) && _.Key?.ActionName?.Split('_')?[0].ToLower() == ActionType.CreatePost.GetDescription().ToLower()).SquashMe(),
					CommentingActions = schedulerHistory.Where(_ => _.Key.Type != typeof(TimelineDeletedItem) && _.Key?.ActionName?.Split('_')?[0].ToLower() == ActionType.CreateCommentMedia.GetDescription().ToLower()).SquashMe(),
					FollowUserActions = schedulerHistory.Where(_ => _.Key.Type != typeof(TimelineDeletedItem) && _.Key?.ActionName?.Split('_')?[0].ToLower() == ActionType.FollowUser.GetDescription().ToLower()).SquashMe(),
					LikeMediaActions = schedulerHistory.Where(_ => _.Key.Type != typeof(TimelineDeletedItem) && _.Key?.ActionName?.Split('_')?[0].ToLower() == ActionType.LikePost.GetDescription().ToLower()).SquashMe()
				};
			}
			return new ScheduleWindow();
		}
		private ScheduleWindow GetTodaysScheduleWindow (string accountId, string instagramId = null, int limit = 1000)
		{
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
					All = schedulerHistory.Where(_=>_.Key.TimelineType != typeof(TimelineDeletedItem) && _.Key?.ActionName?.Split('_')?[0].ToLower() != ActionType.CreatePost.GetDescription().ToLower()).SquashMe(),
					CreatePostActions = schedulerHistory.Where(_ => _.Key.TimelineType != typeof(TimelineDeletedItem) && _.Key?.ActionName?.Split('_')?[0].ToLower() == ActionType.CreatePost.GetDescription().ToLower()).SquashMe(),
					CommentingActions = schedulerHistory.Where(_ => _.Key.TimelineType != typeof(TimelineDeletedItem) && _.Key?.ActionName?.Split('_')?[0].ToLower() == ActionType.CreateCommentMedia.GetDescription().ToLower()).SquashMe(),
					FollowUserActions = schedulerHistory.Where(_ => _.Key.TimelineType != typeof(TimelineDeletedItem) && _.Key?.ActionName?.Split('_')?[0].ToLower() == ActionType.FollowUser.GetDescription().ToLower()).SquashMe(),
					LikeMediaActions = schedulerHistory.Where(_ => _.Key.TimelineType != typeof(TimelineDeletedItem) && _.Key?.ActionName?.Split('_')?[0].ToLower() == ActionType.LikePost.GetDescription().ToLower()).SquashMe()
				};
			}
			return null;
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
					All = schedulerHistory.SquashMe(),
					CreatePostActions = schedulerHistory.Where(_ => _.Key.TimelineType != typeof(TimelineDeletedItem) && _.Key?.ActionName?.Split('_')?[0].ToLower() == ActionType.CreatePost.GetDescription().ToLower()).SquashMe(),
					CommentingActions = schedulerHistory.Where(_ => _.Key.TimelineType != typeof(TimelineDeletedItem) && _.Key?.ActionName?.Split('_')?[0].ToLower() == ActionType.CreateCommentMedia.GetDescription().ToLower()).SquashMe(),
					FollowUserActions = schedulerHistory.Where(_ => _.Key.TimelineType != typeof(TimelineDeletedItem) && _.Key?.ActionName?.Split('_')?[0].ToLower() == ActionType.FollowUser.GetDescription().ToLower()).SquashMe(),
					LikeMediaActions = schedulerHistory.Where(_ => _.Key.TimelineType != typeof(TimelineDeletedItem) && _.Key?.ActionName?.Split('_')?[0].ToLower() == ActionType.LikePost.GetDescription().ToLower()).SquashMe()
				}; 
			}
			return new ScheduleWindow();
		}
		private IEnumerable<ResultBase<TimelineItem>> GetEveryActionForToday (string accid, ActionType action, string instaacc = null, int limit = 5000, bool isHourly = false)
		{
			ScheduleWindow schedule = new ScheduleWindow();
			if (isHourly)
			{
				schedule = GetHourCompletedActions(accid,instaacc,limit);
			}
			else
			{
				schedule = GetDayCompletedActions(accid, instaacc,limit);
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
		public async Task<AgentResponse> StartAgent(string accountId, string instagramAccountId, string accessToken)
		{
			var _instaAccount = await _instagramAccountLogic.GetInstagramAccountShort(accountId, instagramAccountId);
			UserStoreDetails _userStoreDetails = new UserStoreDetails();

			//if (_instaAccount.AgentSettings.Date.HasValue) { 
			//	if(DateTime.UtcNow > _instaAccount.AgentSettings.Date.Value)
			//	{
			//		_instaAccount.AgentSettings.Date = DateTime.UtcNow;
			//		_instaAccount.AgentSettings.ActionStates = null;
			//	}
			//}
			//else
			//{
			//	_instaAccount.AgentSettings.Date = DateTime.UtcNow;
			//}

			await _instagramAccountLogic.PartialUpdateInstagramAccount(accountId,instagramAccountId, 
				new InstagramAccountModel
				{ 
					AgentSettings = new AgentSettings
					{
						AgentState = (int) AgentState.Running,
						//ActionStates = _instaAccount.AgentSettings.ActionStates
					}
				});

			_instaAccount = await _instagramAccountLogic.GetInstagramAccountShort(accountId, instagramAccountId);

			_userStoreDetails.AddUpdateUser(accountId, instagramAccountId, accessToken);
			if(_instaAccount==null) return new AgentResponse
			{
				HttpStatus = HttpStatusCode.NotFound,
				Message = "Account not found"
			};

			if (_instaAccount.DateAdded.HasValue)
			{
				if (DateTime.UtcNow.Subtract(_instaAccount.DateAdded.Value).TotalDays < 7)
				{
					LikeActionOptions.LikeActionDailyLimit = new Range(LikeCommentActionOptions.LikeActionDailyLimit.Min / 2,LikeCommentActionOptions.LikeActionDailyLimit.Max / 2);
					LikeActionOptions.LikeActionHourlyLimit = new Range(LikeCommentActionOptions.LikeActionHourlyLimit.Min / 2, LikeCommentActionOptions.LikeActionHourlyLimit.Max / 2);

					CommentingActionOptions.CommentingActionDailyLimit = new Range(CommentingActionOptions.CommentingActionDailyLimit.Min / 2, CommentingActionOptions.CommentingActionDailyLimit.Max / 2);
					CommentingActionOptions.CommentingActionHourlyLimit = new Range(CommentingActionOptions.CommentingActionHourlyLimit.Min / 2, CommentingActionOptions.CommentingActionHourlyLimit.Max / 2);

					LikeCommentActionOptions.LikeActionDailyLimit = new Range(LikeCommentActionOptions.LikeActionDailyLimit.Min / 2,LikeCommentActionOptions.LikeActionDailyLimit.Max / 2);
					LikeCommentActionOptions.LikeActionHourlyLimit = new Range(LikeCommentActionOptions.LikeActionHourlyLimit.Min / 2, LikeCommentActionOptions.LikeActionHourlyLimit.Max / 2);

					PostActionOptions.CreatePostActionDailyLimit = new Range(PostActionOptions.CreatePostActionDailyLimit.Min / 2, PostActionOptions.CreatePostActionDailyLimit.Max / 2);
					PostActionOptions.CreatePostActionHourlyLimit = new Range(PostActionOptions.CreatePostActionHourlyLimit.Min / 2, PostActionOptions.CreatePostActionHourlyLimit.Max / 2);

					//account new
					//use warming up limits
				}
				else
				{
					//account old
				}
			}
			_userStoreDetails.OInstagramAccountUsername = _instaAccount.Username;
			var profile = await _profileLogic.GetProfile(accountId,instagramAccountId); 
			if (profile == null) return null;
			DateTime nextAvaliableDate = PickAGoodTime(accountId, instagramAccountId);

			//if(_instaAccount.AgentSettings.ActionStates!=null)
			//	CurrentStates = _instaAccount.AgentSettings.ActionStates;

			#region Action Initialise
			ActionsContainerManager actionsContainerManager = new ActionsContainerManager();

			var likeAction = ActionsManager.Begin.Commit(ActionType.LikePost, _contentManager,_heartbeatLogic, profile)
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
			var likeScheduleOptions = new LikeActionOptions(nextAvaliableDate.AddMinutes(SecureRandom.Next(1,4)), LikeActionType.Any);
			var postScheduleOptions = new PostActionOptions(nextAvaliableDate.AddMinutes(SecureRandom.Next(1, 5))) { ImageFetchLimit = 20};
			var followScheduleOptions = new FollowActionOptions(nextAvaliableDate.AddMinutes(SecureRandom.Next(1, 4)), FollowActionType.Any);
			var commentScheduleOptions = new CommentingActionOptions(nextAvaliableDate.AddMinutes(SecureRandom.Next(1, 4)), CommentingActionType.Any);
			var likecommentScheduleOptions = new LikeCommentActionOptions(nextAvaliableDate.AddMinutes(SecureRandom.Next(4)), LikeCommentActionType.Any);
			actionsContainerManager.AddAction(postAction,postScheduleOptions,0.10);
			actionsContainerManager.AddAction(likeAction,likeScheduleOptions,0.45);
			actionsContainerManager.AddAction(followAction,followScheduleOptions,0.15);
			actionsContainerManager.AddAction(commentAction,commentScheduleOptions,0.10);
			actionsContainerManager.AddAction(likeCommentAction, likecommentScheduleOptions, 0.20);
			#endregion
			#region Initialise Timers
			bool _IsQueuingAction = false;
			bool _IsRunningAction = false;
			//bool _IsSleeping = false;
			bool _IsAddingAction = true;
			Timer __AccountStateRefresher__ = new Timer(TimeSpan.FromSeconds(15).TotalMilliseconds);
			Timer __InstanceRefresher__ = new Timer(TimeSpan.FromSeconds(0.5).TotalMilliseconds);
			Timer __TokenRefresh__ = new Timer(TimeSpan.FromMinutes(45).TotalMilliseconds);
			Timer __Unfollowschedule__ = new Timer(TimeSpan.FromHours(2).TotalMilliseconds);
			__AccountStateRefresher__.Start();
			__InstanceRefresher__.Start();
			__Unfollowschedule__.Start();
			__TokenRefresh__.Start();

			_userStoreDetails.ORefreshToken = _authHandler.GetUserByUsername(_userStoreDetails.OAccountId).GetAwaiter().GetResult().Tokens.Where(_ => _.Name == "refresh_token").FirstOrDefault().Value;
			#endregion
			#region Timer Actions
			__Unfollowschedule__.Elapsed += (o,e) =>
			{
				var time = PickAGoodTime(accountId, instagramAccountId);
				if (_instaAccount.FollowingCount.HasValue)
				{
					var res = ActionsManager.Begin.Commit(ActionType.UnFollowUser, _contentManager, _heartbeatLogic, profile)
					.IncludeStrategy(new UnFollowStrategySettings
					{
						UnFollowStrategy = UnFollowStrategyType.LeastEngagingN,
						NumberOfUnfollows = (int)(_instaAccount.FollowingCount.Value * 0.40),
						OffsetPerAction = TimeSpan.FromSeconds(20)
					})
					.IncludeUser(_userStoreDetails)
					.Push(new UnfollowActionOptions(time.AddSeconds(FollowActionOptions.TimeFrameSeconds.Max)));
					if (res.IsSuccesful)
					{
						foreach(var r in res.Results)
							_timelineLogic.AddEventToTimeline(r.ActionName, r.Data, r.ExecutionTime);
					}
				}
				
			};	
			__TokenRefresh__.Elapsed += (o, e) =>
			{
				var res = _authHandler.RefreshLogin(_userStoreDetails.ORefreshToken,_userStoreDetails.OAccountId).GetAwaiter().GetResult();
				if(res.IsSuccesful)
				{
					var newtoken = res.Results.AuthenticationResult.IdToken;
					_userStoreDetails.OAccessToken = newtoken;
					likeAction.IncludeUser(_userStoreDetails);
					postAction.IncludeUser(_userStoreDetails);
					followAction.IncludeUser(_userStoreDetails);
					commentAction.IncludeUser(_userStoreDetails);

					//Update currently Queued Items
					var items = _timelineLogic.GetScheduledEventsForUser(_userStoreDetails.OAccountId, _userStoreDetails.OInstagramAccountUser).ToList();
					items.ForEach(_ =>
					{
						_timelineLogic.DeleteEvent(_.ItemId);
						_.User = _userStoreDetails;
						_timelineLogic.AddEventToTimeline(_.ActionName, _.Rest, _.EnqueueTime.Value.AddSeconds(30));
					});
				}
			};
			__AccountStateRefresher__.Elapsed += async(o, e) =>
			{
				var totalforuser = GetTodaysScheduleWindow(accountId,instagramAccountId);
				if (totalforuser != null)
				{
					if (totalforuser.All.Count() > 200)
						_instaAccount.AgentSettings.AgentState = (int) AgentState.Sleeping;
					else if (totalforuser.All.Count() <= 0)
						_instaAccount.AgentSettings.AgentState = (int) AgentState.Running;
				}
				else
					_instaAccount.AgentSettings.AgentState = (int) AgentState.Running;

				await _instagramAccountLogic.PartialUpdateInstagramAccount(accountId, instagramAccountId, new InstagramAccountModel
				{
					AgentSettings = new AgentSettings
					{
						AgentState = _instaAccount.AgentSettings.AgentState,
						//ActionStates = CurrentStates
					}
				});
				_instaAccount = await _instagramAccountLogic.GetInstagramAccountShort(accountId, instagramAccountId);
			};

			__InstanceRefresher__.Elapsed += (o, e) =>
			{
				if(_instaAccount==null) return;
				if(_instaAccount.AgentSettings.AgentState == (int) AgentState.Running) {
					if (_IsAddingAction) {
						_IsAddingAction = false;
						_= Task.Run(() => { 
							var nominatedAction = actionsContainerManager.GetRandomAction();
							actionsContainerManager.AddWork(nominatedAction);
							Task.Delay(TimeSpan.FromSeconds(1));
						}).ContinueWith(s=> _IsAddingAction = true);
					}
					if (!_IsRunningAction) {
						_IsRunningAction = true;
						_ = Task.Run(() => {
							actionsContainerManager.RunAction();
						}).ContinueWith(x=> _IsRunningAction = false);
					}
					if (!_IsQueuingAction) {
						_IsQueuingAction = true;
						var finishedAction = actionsContainerManager.GetFinishedActions()?.DistinctBy(d=>d.Data);
						if (finishedAction != null)
						{
							Parallel.ForEach(finishedAction, _ =>
							{
								string actionName = _.ActionName.Split('_')[0].ToLower();
								var atype = actionName.GetValueFromDescription<ActionType>();
								var timeSett = actionsContainerManager.FindActionLimit(atype);

								nextAvaliableDate = PickAGoodTime(accountId, instagramAccountId, actionName.GetValueFromDescription<ActionType>());
								actionsContainerManager.HasMetTimeLimit();
								if (nextAvaliableDate != null)
								{
									_.ExecutionTime = nextAvaliableDate.AddSeconds(timeSett.Value.Max);
									var res_ = AddToTimeline(_);
									if (res_.HasCompleted)
									{
										//switch (atype)
										//{
										//	case ActionType.CreatePost:
										//		CurrentStates.NumberOfPostsPosted++;
										//		CurrentStates.NumberOfPostsPostedHourly++;
										//		break;
										//	case ActionType.FollowUser:
										//		CurrentStates.NumberOfPeopleFollowed++;
										//		CurrentStates.NumberOfPeopleFollowedHourly++;
										//		break;
										//	case ActionType.LikeComment:
										//		CurrentStates.NumberOfCommentsLiked++;
										//		CurrentStates.NumberOfCommentsLikedHourly++;
										//		break;
										//	case ActionType.LikePost:
										//		CurrentStates.NumberOfPostsLiked++;
										//		CurrentStates.NumberOfPostsLikedHourly++;
										//		break;
										//	case ActionType.CreateCommentMedia:
										//		CurrentStates.NumberOfCommentsMade++;
										//		CurrentStates.NumberOfCommentsMadeHourly++;
										//		break;
										//}
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
						_IsQueuingAction = false;
					}
				}
				else if(_instaAccount.AgentSettings.AgentState == (int)AgentState.Sleeping)
				{
					//var time = PickAGoodTime(accountId, instagramAccountId);
					//var profileChecker = ActionsManager.Begin.Commit(ActionType.MaintainAccount, _contentManager, _heartbeatLogic, profile)
					//	.IncludeStrategy(new AccountCheckerStrategySettings
					//	{
					//		AccountCheckerStrategy = AccountCheckerStrategy.Default,
					//		OffsetPerAction = TimeSpan.FromSeconds(25)
					//	})
					//	.IncludeUser(_userStoreDetails)
					//	.Push(new AccountCheckerActionOptions() { ExecutionTime = time });
					//if (profileChecker.IsSuccesful)
					//{
					//	foreach (var ch in profileChecker.Results)
					//		_timelineLogic.AddEventToTimeline(ch.ActionName, ch.Data, ch.ExecutionTime);
					//}
				}
				else if(_instaAccount.AgentSettings.AgentState == (int) AgentState.Stopped){
					__InstanceRefresher__.Stop();
				}
			};
			#endregion

			return new AgentResponse
			{
				HttpStatus = HttpStatusCode.OK,
				Message = "Done"
			};
		}
		public async Task<AgentResponse> StopAgent(string accountId, string instagramAccountId)
		{
			try { 
				await _instagramAccountLogic.PartialUpdateInstagramAccount(accountId,instagramAccountId, 
					new InstagramAccountModel
					{
						AgentSettings = new AgentSettings
						{
							AgentState = (int) AgentState.Stopped,
						}
					});
				return new AgentResponse
				{
					HttpStatus = HttpStatusCode.OK,
					Message = "Stopped"
				};
			}
			catch(Exception ee)
			{
				return new AgentResponse
				{
					HttpStatus = HttpStatusCode.InternalServerError,
					Message = ee.Message
				};
			}
		}
	}
}
