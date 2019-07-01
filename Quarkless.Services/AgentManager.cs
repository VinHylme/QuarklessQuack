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
using System.Net;
using System.Threading.Tasks;
using System.Linq;
using Quarkless.Services.Extensions;
using QuarklessLogic.Logic.InstagramAccountLogic;
using System.Timers;
using QuarklessLogic.Logic.AuthLogic.Auth;
using QuarklessContexts.Models.InstagramAccounts;
using System.Reflection;

namespace Quarkless.Services
{
	static class ActionNames
	{
		public const string CreatePhoto = "createphoto";
		public const string CreateVideo = "createvideo";
		public const string Comment = "comment";
		public const string LikeMedia = "likemedia";
		public const string FollowUser = "followuser";
	}
	public class AgentRespnse
	{
		public HttpStatusCode HttpStatus { get; set; }
		public string Message { get; set; }
	}
	struct ScheduleWindow
	{
		public IEnumerable<ResultBase<TimelineItem>> CreateImageActions;
		public IEnumerable<ResultBase<TimelineItem>> CreateVideoActions;
		public IEnumerable<ResultBase<TimelineItem>> CommentingActions;
		public IEnumerable<ResultBase<TimelineItem>> LikeMediaActions;
		public IEnumerable<ResultBase<TimelineItem>> FollowUserActions;
		public IEnumerable<ResultBase<TimelineItem>> All;
	}

	public class AgentManager : IAgentManager
	{
		private readonly IInstagramAccountLogic _instagramAccountLogic;
		private readonly IContentManager _contentManager;
		private readonly ITimelineLogic _timelineLogic;
		private readonly IAuthHandler _authHandler;
		public AgentManager(IInstagramAccountLogic instagramAccountLogic, IContentManager contentManager,
			ITimelineLogic timelineLogic, IAuthHandler authHandler)
		{
			_timelineLogic = timelineLogic;
			_instagramAccountLogic = instagramAccountLogic;
			_contentManager = contentManager;
			_authHandler = authHandler;
		}

		private AddEventResponse AddToTimeline(TimelineEventModel events)
		{
			if (events != null) {
				try
					{
						var actionBase = events.ActionName.Split('_')[0].ToLower();
						var actionsScheduledForTheDay = GetEveryActionForToday(events.Data.User.OAccountId,actionBase,events.Data.User.OInstagramAccountUser);
						var actionsScheduledForTheLastHour = GetEveryActionForToday(events.Data.User.OAccountId, actionBase, events.Data.User.OInstagramAccountUser,isHourly:true);
							
						if (actionBase.Contains(ActionNames.CreatePhoto))
						{
							if (actionsScheduledForTheDay.Count() >= ImageActionOptions.CreatePhotoActionDailyLimit.Max)
							{
								return new AddEventResponse
								{
									Event = events,
									ContainsErrors = false,
									HasCompleted = false,
									DailyLimitReached = true
								};
							}
							if (actionsScheduledForTheLastHour.Count() >= ImageActionOptions.CreatePhotoActionHourlyLimit.Max)
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
						else if (actionBase.Contains(ActionNames.CreateVideo))
						{
							if (actionsScheduledForTheDay.Count() >= VideoActionOptions.CreateVideoActionDailyLimit.Max)
							{
								return new AddEventResponse
								{
									Event = events,
									ContainsErrors = false,
									HasCompleted = false,
									DailyLimitReached = true
								};
							}
							if (actionsScheduledForTheLastHour.Count() >= VideoActionOptions.CreateVideoActionHourlyLimit.Max)
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
						else if (actionBase.Contains(ActionNames.Comment))
						{
							if (actionsScheduledForTheDay.Count() >= CommentingActionOptions.CommentingActionDailyLimit.Max)
							{
								return new AddEventResponse
								{
									Event = events,
									ContainsErrors = false,
									HasCompleted = false,
									DailyLimitReached = true
								};
							}
							if (actionsScheduledForTheLastHour.Count() >= CommentingActionOptions.CommentingActionHourlyLimit.Max)
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
						else if (actionBase.Contains(ActionNames.LikeMedia))
						{
							if (actionsScheduledForTheDay.Count() >= LikeActionOptions.LikeActionDailyLimit.Max)
							{
								return new AddEventResponse
								{
									Event = events,
									ContainsErrors = false,
									HasCompleted = false,
									DailyLimitReached = true
								};
							}
							if (actionsScheduledForTheLastHour.Count() >= LikeActionOptions.LikeActionHourlyLimit.Max)
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
						else if (actionBase.Contains(ActionNames.FollowUser))
						{
							if (actionsScheduledForTheDay.Count() >= FollowActionOptions.FollowActionDailyLimit.Max)
							{
								return new AddEventResponse
								{
									Event = events,
									ContainsErrors = false,
									HasCompleted = false,
									DailyLimitReached = true
								};
							}
							if (actionsScheduledForTheLastHour.Count() >= FollowActionOptions.FollowActionHourlyLimit.Max)
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
		private bool RunAction(IActionCommit action, IActionOptions actionOptions)
		{
			var executeAction = action.Push(actionOptions).ToList();
			if (executeAction != null)
			{
				executeAction.ForEach(_ =>
				{
					if(_!=null)
						AddToTimeline(_);
				});
			}
			return false;
		}
		private ScheduleWindow GetTodaysScheduleWindow (string accountId, TimelineDateType timelineDateType, string instagramId = null, int limit = 1000)
		{
			var todays = _timelineLogic.GetAllEventsForUser(accountId, DateTime.UtcNow, instaId: instagramId, 
				limit: limit,timelineDateType:timelineDateType);

			var schedulerHistory = todays.OrderBy(_ => _.Response.EnqueueTime).GroupBy(_ => new { _.TimelineType, _.Response.ActionName });

			return new ScheduleWindow
			{
				All = schedulerHistory.SquashMe(),
				CreateImageActions = schedulerHistory.Where(_ => _.Key.TimelineType != typeof(TimelineDeletedItem) && _.Key?.ActionName?.Split('_')?[0].ToLower() == ActionNames.CreatePhoto.ToLower()).SquashMe(),
				CommentingActions = schedulerHistory.Where(_ => _.Key.TimelineType != typeof(TimelineDeletedItem) && _.Key?.ActionName?.Split('_')?[0].ToLower() == ActionNames.Comment.ToLower()).SquashMe(),
				CreateVideoActions = schedulerHistory.Where(_ => _.Key.TimelineType != typeof(TimelineDeletedItem) && _.Key?.ActionName?.Split('_')?[0].ToLower() == ActionNames.CreateVideo.ToLower()).SquashMe(),
				FollowUserActions = schedulerHistory.Where(_ => _.Key.TimelineType != typeof(TimelineDeletedItem) && _.Key?.ActionName?.Split('_')?[0].ToLower() == ActionNames.FollowUser.ToLower()).SquashMe(),
				LikeMediaActions = schedulerHistory.Where(_ => _.Key.TimelineType != typeof(TimelineDeletedItem) && _.Key?.ActionName?.Split('_')?[0].ToLower() == ActionNames.LikeMedia.ToLower()).SquashMe()
			};
		}
		private ScheduleWindow GetLastHoursScheduleWindow(string accountId, TimelineDateType timelineDateType, string instagramId = null, int limit = 1000)
		{
			var todays = _timelineLogic.GetAllEventsForUser(accountId, DateTime.UtcNow, endDate: DateTime.UtcNow.AddHours(-1),
				instaId: instagramId, limit: limit, timelineDateType:timelineDateType);

			var schedulerHistory = todays.OrderBy(_ => _.Response.EnqueueTime).GroupBy(_ => new { _.TimelineType, _.Response.ActionName });

			return new ScheduleWindow
			{
				All = schedulerHistory.SquashMe(),
				CreateImageActions = schedulerHistory.Where(_ => _.Key.TimelineType != typeof(TimelineDeletedItem) && _.Key?.ActionName?.Split('_')?[0].ToLower() == ActionNames.CreatePhoto.ToLower()).SquashMe(),
				CommentingActions = schedulerHistory.Where(_ => _.Key.TimelineType != typeof(TimelineDeletedItem) && _.Key?.ActionName?.Split('_')?[0].ToLower() == ActionNames.Comment.ToLower()).SquashMe(),
				CreateVideoActions = schedulerHistory.Where(_ => _.Key.TimelineType != typeof(TimelineDeletedItem) && _.Key?.ActionName?.Split('_')?[0].ToLower() == ActionNames.CreateVideo.ToLower()).SquashMe(),
				FollowUserActions = schedulerHistory.Where(_ => _.Key.TimelineType != typeof(TimelineDeletedItem) && _.Key?.ActionName?.Split('_')?[0].ToLower() == ActionNames.FollowUser.ToLower()).SquashMe(),
				LikeMediaActions = schedulerHistory.Where(_ => _.Key.TimelineType != typeof(TimelineDeletedItem) && _.Key?.ActionName?.Split('_')?[0].ToLower() == ActionNames.LikeMedia.ToLower()).SquashMe()
			};
		}
		private IEnumerable<ResultBase<TimelineItem>> GetEveryActionForToday (string accid, string action, string instaacc = null, int limit = 1000, bool isHourly = false)
		{
			ScheduleWindow schedule = new ScheduleWindow();
			if (isHourly)
			{
				schedule = GetLastHoursScheduleWindow(accid,TimelineDateType.Both,instaacc,limit);
			}
			else
			{
				schedule = GetTodaysScheduleWindow(accid,TimelineDateType.Both,instaacc,limit);
			}
			switch (action)
			{
				case ActionNames.LikeMedia:
					return schedule.LikeMediaActions;
				case ActionNames.Comment:
					return schedule.CommentingActions;
				case ActionNames.CreatePhoto:
					return schedule.CreateImageActions;
				case ActionNames.CreateVideo:
					return schedule.CreateVideoActions;
				case ActionNames.FollowUser:
					return schedule.FollowUserActions;
			}

			return null;
		}
		public async Task<AgentRespnse> StartAgent(string accountId, string instagramAccountId, string accessToken)
		{
			UserStoreDetails _userStoreDetails = new UserStoreDetails();
			await _instagramAccountLogic.PartialUpdateInstagramAccount(accountId,instagramAccountId, new InstagramAccountModel{ AgentState = true });
			_userStoreDetails.AddUpdateUser(accountId, instagramAccountId, accessToken);

			var context = _contentManager.SetUser(_userStoreDetails);
			_userStoreDetails.OInstagramAccountUsername = context.InstagramAccount.Username;

			var profile = context.Profile; 
			if (profile == null) return null;

			ActionsContainerManager actionsContainerManager = new ActionsContainerManager();

			var likeAction = ActionsManager.Begin.Commit(ActionType.LikePost, _contentManager, profile)
				.IncludeStrategy(new LikeStrategySettings()
				{
					LikeStrategy = LikeStrategyType.Default,
				})
				.IncludeUser(_userStoreDetails);
			var imageAction = ActionsManager.Begin.Commit(ActionType.CreatePostTypeImage, _contentManager, profile)
				.IncludeStrategy(new ImageStrategySettings
				{
					ImageStrategyType = ImageStrategyType.Default
				})
				.IncludeUser(_userStoreDetails);
			var followAction = ActionsManager.Begin.Commit(ActionType.FollowUser, _contentManager, profile)
				.IncludeStrategy(new FollowStrategySettings
				{
					FollowStrategy = FollowStrategyType.Default,
				})
				.IncludeUser(_userStoreDetails);
			var commentAction = ActionsManager.Begin.Commit(ActionType.CreateCommentMedia, _contentManager, profile)
				.IncludeStrategy(new CommentingStrategySettings
				{
					CommentingStrategy = CommentingStrategy.Default,
				})
				.IncludeUser(_userStoreDetails);

			DateTime? nextAvaliableDate = PickAGoodTime();
			DateTime? PickAGoodTime(TimeSpan? actionTime = null) { 
				var sft = _timelineLogic.GetScheduledEventsForUserByDate(accountId,DateTime.UtcNow,instaId:instagramAccountId,limit:5000,timelineDateType:TimelineDateType.Forward);
				var datesPlanned = sft.Select(_=>_.EnqueueTime);
				if (datesPlanned != null && datesPlanned.Count()>0) { 
					DateTime current = DateTime.UtcNow;
					if (actionTime != null)
					{
						for (int x = 0; x < datesPlanned.Count(); x++)
						{
							var diff = datesPlanned.ElementAt(x) - current;

							if(diff.Value.TotalMilliseconds > actionTime.Value.TotalMilliseconds)
							{

								if((actionTime.Value.TotalMilliseconds/diff.Value.TotalMilliseconds) > 0.5)
								{
									return datesPlanned.ElementAt(x).Value - actionTime.Value;
								}
								return datesPlanned.ElementAt(x).Value + (actionTime.Value);
							}
							continue;
						}
					}

					var difference = datesPlanned.Where(_ => _ != null).Max(_ => _ - current);
					var position = datesPlanned.ToList().FindIndex(n => n - current == difference);
					if (difference.Value.TotalMinutes > TimeSpan.FromMinutes(55).TotalMinutes)
					{
						return null;
					}
					return datesPlanned.ElementAt(position).Value;
				}
				else
				{
					return DateTime.UtcNow;
				}
			}

			//Initial Execution
			var likeScheduleOptions = new LikeActionOptions(nextAvaliableDate.Value.AddMinutes(SecureRandom.Next(1,4)), LikeActionType.Any);
			var imageScheduleOptions = new ImageActionOptions(nextAvaliableDate.Value.AddMinutes(SecureRandom.Next(1, 5))) { ImageFetchLimit = 20};
			var followScheduleOptions = new FollowActionOptions(nextAvaliableDate.Value.AddMinutes(SecureRandom.Next(1, 4)), FollowActionType.Any);
			var commentScheduleOptions = new CommentingActionOptions(nextAvaliableDate.Value.AddMinutes(SecureRandom.Next(1, 4)), CommentingActionType.Any);
			
			actionsContainerManager.AddAction(imageAction,imageScheduleOptions,0.10);
			actionsContainerManager.AddAction(likeAction,likeScheduleOptions,0.50);
			actionsContainerManager.AddAction(followAction,followScheduleOptions,0.15);
			actionsContainerManager.AddAction(commentAction,commentScheduleOptions,0.25);

			Timer instanceRefresher = new Timer(TimeSpan.FromSeconds(2.5).TotalMilliseconds);
			Timer tokenRefresh = new Timer(TimeSpan.FromMinutes(50).TotalMilliseconds);

			_userStoreDetails.ORefreshToken = _authHandler.GetUserByUsername(_userStoreDetails.OAccountId).GetAwaiter().GetResult().Tokens.Where(_ => _.Name == "refresh_token").FirstOrDefault().Value;

			tokenRefresh.Start();
			tokenRefresh.Elapsed += (o, e) =>
			{
				var res = _authHandler.RefreshLogin(_userStoreDetails.ORefreshToken,_userStoreDetails.OAccountId).GetAwaiter().GetResult();
				if(res.StatusCode == HttpStatusCode.OK || res.StatusCode == HttpStatusCode.Accepted)
				{
					var newtoken = res.Results.AuthenticationResult.IdToken;
					_userStoreDetails.OAccessToken = newtoken;
					likeAction.IncludeUser(_userStoreDetails);
					imageAction.IncludeUser(_userStoreDetails);
					followAction.IncludeUser(_userStoreDetails);
					commentAction.IncludeUser(_userStoreDetails);

					//Update currently Queued Items
					var items = _timelineLogic.GetScheduledEventsForUser(_userStoreDetails.OAccountId,_userStoreDetails.OInstagramAccountUser).ToList();
					items.ForEach(_ =>
					{
						_timelineLogic.DeleteEvent(_.ItemId);
						_.User = _userStoreDetails;
						_timelineLogic.AddEventToTimeline(_.ActionName,_.Rest,_.EnqueueTime.Value.AddSeconds(30));
					});
				}
			};
			bool started = false;
			instanceRefresher.Start();
			instanceRefresher.Elapsed += async(o, e) =>
			{
				context.InstagramAccount = await _instagramAccountLogic.GetInstagramAccountShort(accountId, instagramAccountId);
				if(context.InstagramAccount.AgentState == true) {

					_= Task.Run(() => { 
						var nominatedAction = actionsContainerManager.GetRandomAction();
						actionsContainerManager.AddWork(nominatedAction);
					});

					_ = Task.Run(() => {
						actionsContainerManager.RunAction();
					});

					if (!started) { 
						started = true;
						var finishedAction = actionsContainerManager.GetFinishedActions();
						if (finishedAction != null)
						{
							Parallel.ForEach(finishedAction, _ =>
							{
								var timeSett = actionsContainerManager.FindActionLimit(_.ActionName.Split('_')[0].ToLower());
								nextAvaliableDate = PickAGoodTime(TimeSpan.FromSeconds(timeSett.Value.Max));
								if (nextAvaliableDate != null)
								{
									_.ExecutionTime = nextAvaliableDate.Value.AddSeconds(Average(timeSett.Value.Min, timeSett.Value.Max));
									AddToTimeline(_);
								}
							});		
						}
						started = false;
					}

				}
				else {
					instanceRefresher.Stop();
				}
			};
			return new AgentRespnse
			{
				HttpStatus = HttpStatusCode.OK,
				Message = "Done"
			};
		}
		public AgentRespnse StopAgent(string accountId, string instagramAccountId)
		{
			try { 
				_instagramAccountLogic.PartialUpdateInstagramAccount(accountId,instagramAccountId, new InstagramAccountModel{AgentState = false});
				return new AgentRespnse
				{
					HttpStatus = HttpStatusCode.OK,
					Message = "Stopped"
				};
			}
			catch(Exception ee)
			{
				return new AgentRespnse
				{
					HttpStatus = HttpStatusCode.InternalServerError,
					Message = ee.Message
				};
			}
		}
		private int Average(params int[] vals)
		{
			return vals.Sum()/vals.Length;
		}
	}
}
