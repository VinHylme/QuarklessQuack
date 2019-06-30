using Quarkless.Services.ActionBuilders.EngageActions;
using Quarkless.Services.Factories;
using Quarkless.Services.Interfaces;
using Quarkless.Services.Interfaces.Actions;
using Quarkless.Services.StrategyBuilders;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models;
using QuarklessContexts.Models.Timeline;
using QuarklessLogic.Handlers.ClientProvider;
using QuarklessLogic.Logic.ProfileLogic;
using QuarklessLogic.ServicesLogic.TimelineServiceLogic.TimelineLogic;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Globalization;
using Quarkless.Services.Extensions;
using QuarklessLogic.Logic.InstagramAccountLogic;

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
	class ActionContainer
	{
		public IActionOptions ActionOptions { get; set; }
		public Action<IActionOptions> Action_ { get; set; }
	}

	public class AgentManager : IAgentManager
	{
		private readonly IInstagramAccountLogic _instagramAccountLogic;
		private readonly IContentManager _contentManager;
		private readonly IAPIClientContext _clientContext;
		private readonly IUserStoreDetails _userStoreDetails;
		private readonly ITimelineLogic _timelineLogic;
		public AgentManager(IInstagramAccountLogic instagramAccountLogic,
			IContentManager contentManager, IAPIClientContext clientContext,
			IUserStoreDetails userStoreDetails, ITimelineLogic timelineLogic)
		{
			_timelineLogic = timelineLogic;
			_instagramAccountLogic = instagramAccountLogic;
			_contentManager = contentManager;
			_clientContext = clientContext;
			_userStoreDetails = userStoreDetails;
		}
		private IAPIClientContainer GetContext(string accountId, string instagramAccountId)
		{
			return new APIClientContainer(_clientContext, accountId, instagramAccountId);
		}
		private IEnumerable<AddEventResponse> AddToTimeline(IEnumerable<TimelineEventModel> events)
		{
			if (events != null) {
				List<AddEventResponse> events_response = new List<AddEventResponse>();
				foreach(var event_ in events)
				{
					if (event_ != null)
					{
						try
						{
							var actionBase = event_.ActionName.Split('_')[0].ToLower();
							var actionsScheduledForTheDay = GetEveryActionForToday(event_.Data.User.OAccountId,actionBase,event_.Data.User.OInstagramAccountUser);
							var actionsScheduledForTheLastHour = GetEveryActionForToday(event_.Data.User.OAccountId, actionBase, event_.Data.User.OInstagramAccountUser,isHourly:true);
							
							if (actionBase.Contains(ActionNames.CreatePhoto))
							{
								if (actionsScheduledForTheDay.Count() >= ImageActionOptions.CreatePhotoActionDailyLimit.Max)
								{
									events_response.Add(new AddEventResponse
									{
										Event = event_,
										ContainsErrors = false,
										HasCompleted = false,
										DailyLimitReached = true
									});
								}
								if (actionsScheduledForTheLastHour.Count() >= ImageActionOptions.CreatePhotoActionHourlyLimit.Max)
								{
									event_.ExecutionTime.AddHours(1.1);
								}
							}
							else if (actionBase.Contains(ActionNames.CreateVideo))
							{
								if (actionsScheduledForTheDay.Count() >= VideoActionOptions.CreateVideoActionDailyLimit.Max)
								{
									events_response.Add(new AddEventResponse
									{
										Event = event_,
										ContainsErrors = false,
										HasCompleted = false,
										DailyLimitReached = true
									});
								}
								if (actionsScheduledForTheLastHour.Count() >= VideoActionOptions.CreateVideoActionHourlyLimit.Max)
								{
									event_.ExecutionTime.AddHours(1.1);
								}
							}
							else if (actionBase.Contains(ActionNames.Comment))
							{
								if (actionsScheduledForTheDay.Count() >= CommentingActionOptions.CommentingActionDailyLimit.Max)
								{
									events_response.Add(new AddEventResponse
									{
										Event = event_,
										ContainsErrors = false,
										HasCompleted = false,
										DailyLimitReached = true
									});
								}
								if (actionsScheduledForTheLastHour.Count() >= CommentingActionOptions.CommentingActionHourlyLimit.Max)
								{
									event_.ExecutionTime.AddHours(1.1);
								}
							}
							else if (actionBase.Contains(ActionNames.LikeMedia))
							{
								if (actionsScheduledForTheDay.Count() >= LikeActionOptions.LikeActionDailyLimit.Max)
								{
									events_response.Add(new AddEventResponse
									{
										Event = event_,
										ContainsErrors = false,
										HasCompleted = false,
										DailyLimitReached = true
									});
								}
								if (actionsScheduledForTheLastHour.Count() >= LikeActionOptions.LikeActionHourlyLimit.Max)
								{
									event_.ExecutionTime.AddHours(1.1);
								}

							}
							else if (actionBase.Contains(ActionNames.FollowUser))
							{
								if (actionsScheduledForTheDay.Count() >= FollowActionOptions.FollowActionDailyLimit.Max)
								{
									events_response.Add(new AddEventResponse
									{
										Event = event_,
										ContainsErrors = false,
										HasCompleted = false,
										DailyLimitReached = true
									});
								}
								if (actionsScheduledForTheLastHour.Count() >= FollowActionOptions.FollowActionHourlyLimit.Max)
								{
									event_.ExecutionTime.AddHours(1.1);
								}
							}

							_timelineLogic.AddEventToTimeline(event_.ActionName, event_.Data, event_.ExecutionTime);
							events_response.Add(new AddEventResponse
							{
								HasCompleted = true,
								Event = event_
							});
						}
						catch (Exception ee)
						{
							events_response.Add(new AddEventResponse
							{
								ContainsErrors = true,
								Event = event_,
								Errors = new TimelineErrorResponse
								{
									Exception = ee,
									Message = ee.Message
								}
							});
						}
					}
				}
				return events_response;
			}
			return null;
		}
		private bool RunAction(IActionCommit action, IActionOptions actionOptions)
		{
			var executeAction = action.Push(actionOptions);
			if (executeAction != null)
			{
				var tryAdd = AddToTimeline(executeAction);
				return tryAdd.All(_=>_.HasCompleted);	
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
			try
			{
				await _instagramAccountLogic.PartialUpdateInstagramAccount(instagramAccountId, new QuarklessContexts.Models.InstagramAccounts.InstagramAccountModel
				{
					AgentState = true
				});
				_userStoreDetails.AddUpdateUser(accountId,instagramAccountId,accessToken);			
				var context = _contentManager.SetUser(_userStoreDetails);
				_userStoreDetails.OInstagramAccountUsername = context.InstagramAccount.Username;
				var profile = context.Profile; 
				if (profile == null) return null;

				var likeAction = ActionsManager.Begin.Commit(ActionType.LikePost,_contentManager,profile,(UserStore)_userStoreDetails)
					.IncludeStrategy(new LikeStrategySettings()
					{
						LikeStrategy = LikeStrategyType.Default,
					});
				var imageAction = ActionsManager.Begin.Commit(ActionType.CreatePostTypeImage, _contentManager, profile, (UserStore)_userStoreDetails)
					.IncludeStrategy(new ImageStrategySettings
					{
						ImageStrategyType = ImageStrategyType.Default
					});
				var followAction = ActionsManager.Begin.Commit(ActionType.FollowUser, _contentManager, profile, (UserStore)_userStoreDetails)
					.IncludeStrategy(new FollowStrategySettings
					{
						FollowStrategy = FollowStrategyType.Default,
					});
				var commentAction = ActionsManager.Begin.Commit(ActionType.CreateCommentMedia, _contentManager, profile, (UserStore)_userStoreDetails)
					.IncludeStrategy(new CommentingStrategySettings
					{
						CommentingStrategy = CommentingStrategy.Default,
					});

				DateTime nextAvaliableDate = PickAGoodTime();
				DateTime PickAGoodTime() { 
					var sft = _timelineLogic.GetScheduledEventsForUserByDate(accountId,DateTime.UtcNow,instaId:instagramAccountId,limit:1000,timelineDateType:TimelineDateType.Forward);
					var datesPlanned = sft.Select(_=>_.EnqueueTime);
					if (datesPlanned != null && datesPlanned.Count()>0) { 
						DateTime current = DateTime.UtcNow;
						var difference = datesPlanned.Where(_=>_!=null).Max(_=>_- current);
						var pos = datesPlanned.ToList().FindIndex(n => n - current == difference);
						return datesPlanned.ElementAt(pos).Value + TimeSpan.FromMinutes(2);;
					}
					else
					{
						return DateTime.UtcNow;
					}
				}

				//Initial Execution
				var likeScheduleOptions = new LikeActionOptions(nextAvaliableDate.AddMinutes(SecureRandom.Next(1,4)), LikeActionType.Any);
				var imageScheduleOptions = new ImageActionOptions(nextAvaliableDate.AddMinutes(SecureRandom.Next(1, 5))) { ImageFetchLimit = 20};
				var followScheduleOptions = new FollowActionOptions(nextAvaliableDate.AddMinutes(SecureRandom.Next(1, 4)), FollowActionType.Any);
				var commentScheduleOptions = new CommentingActionOptions(nextAvaliableDate.AddMinutes(SecureRandom.Next(1, 4)), CommentingActionType.Any);
			
				List<Chance<ActionContainer>> ChanceAction = new List<Chance<ActionContainer>>();
				//add actions in as probabilities
				ChanceAction.Add(new Chance<ActionContainer>
				{
					Object = new ActionContainer
					{
						ActionOptions = imageScheduleOptions,
						Action_ = (o) => RunAction(imageAction,o)
					},
					Probability = 0.0
				});
				ChanceAction.Add(new Chance<ActionContainer>
				{
					Object = new ActionContainer
					{
						ActionOptions = followScheduleOptions,
						Action_ = (o) => RunAction(followAction, o)
					},
					Probability = 0.20
				});
				ChanceAction.Add(new Chance<ActionContainer>
				{
					Object = new ActionContainer
					{
						ActionOptions = commentScheduleOptions,
						Action_ = (o) => RunAction(commentAction, o)
					},
					Probability = 0.30
				});
				ChanceAction.Add(new Chance<ActionContainer>
				{
					Object = new ActionContainer
					{
						ActionOptions = likeScheduleOptions,
						Action_ = (o) => RunAction(likeAction, o)
					},
					Probability = 0.50
				});

				System.Timers.Timer fps = new System.Timers.Timer(TimeSpan.FromSeconds(5).TotalMilliseconds);
				fps.Start();
				fps.Elapsed += async(o, e) =>
				{
					context.InstagramAccount = await _instagramAccountLogic.GetInstagramAccount(accountId, instagramAccountId);
					if(context.InstagramAccount.AgentState == true) { 
						nextAvaliableDate = PickAGoodTime();

						likeScheduleOptions.ExecutionTime = nextAvaliableDate.AddMinutes(1.6);
						imageScheduleOptions.ExecutionTime = nextAvaliableDate.AddMinutes(SecureRandom.Next(3, 6));
						followScheduleOptions.ExecutionTime = nextAvaliableDate.AddMinutes(SecureRandom.Next(2, 4));
						commentScheduleOptions.ExecutionTime = nextAvaliableDate.AddMinutes(SecureRandom.Next(2, 4));

						ChanceAction.Where(_ => _.Object.ActionOptions.GetType() == typeof(LikeActionOptions)).SingleOrDefault().Object.ActionOptions = likeScheduleOptions;
						ChanceAction.Where(_ => _.Object.ActionOptions.GetType() == typeof(FollowActionOptions)).SingleOrDefault().Object.ActionOptions = followScheduleOptions;
						ChanceAction.Where(_ => _.Object.ActionOptions.GetType() == typeof(ImageActionOptions)).SingleOrDefault().Object.ActionOptions = imageScheduleOptions;
						ChanceAction.Where(_ => _.Object.ActionOptions.GetType() == typeof(CommentingActionOptions)).SingleOrDefault().Object.ActionOptions = commentScheduleOptions;
					
						var runAction = SecureRandom.ProbabilityRoll(ChanceAction);
						runAction.Action_(runAction.ActionOptions);
					}
					else { 
						fps.Stop();
					}
				};

				return new AgentRespnse()
				{
					HttpStatus = HttpStatusCode.OK,
					Message = "Started"
				};
			}
			catch(Exception ee)
			{
				await _instagramAccountLogic.PartialUpdateInstagramAccount(instagramAccountId, new QuarklessContexts.Models.InstagramAccounts.InstagramAccountModel
				{
					AgentState = false
				});
				return new AgentRespnse{
					HttpStatus = HttpStatusCode.InternalServerError,
					Message = ee.Message
				};
			}
		}
		public AgentRespnse StopAgent(string instagramAccountId)
		{
			try { 
				_instagramAccountLogic.PartialUpdateInstagramAccount(instagramAccountId, new QuarklessContexts.Models.InstagramAccounts.InstagramAccountModel{AgentState = false});
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
	}
}
