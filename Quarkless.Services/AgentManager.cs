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
		public IEnumerable<TimelineItem> CreateImageActions;
		public IEnumerable<TimelineItem> CreateVideoActions;
		public IEnumerable<TimelineItem> CommentingActions;
		public IEnumerable<TimelineItem> LikeMediaActions;
		public IEnumerable<TimelineItem> FollowUserActions;
	}
	class ActionContainer
	{
		public IActionOptions ActionOptions { get; set; }
		public Action<IActionOptions> Action_ { get; set; }
	}

	public class AgentManager : IAgentManager
	{

		private readonly IProfileLogic _profileLogic;
		private readonly IContentManager _contentManager;
		private readonly IAPIClientContext _clientContext;
		private readonly IUserStoreDetails _userStoreDetails;
		private readonly ITimelineLogic _timelineLogic;
		public AgentManager(IProfileLogic profileLogic,
			IContentManager contentManager, IAPIClientContext clientContext,
			IUserStoreDetails userStoreDetails, ITimelineLogic timelineLogic)
		{
			_timelineLogic = timelineLogic;
			_profileLogic = profileLogic;
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
							var actionsScheduledForTheDay = _timelineLogic.GetScheduledEventsForUserForActionByDate(event_.Data.User.OAccountId, actionBase, event_.ExecutionTime.Date);
							var actionsScheduledForTheNextHour = _timelineLogic.GetScheduledEventsForUserForActionByDate(event_.Data.User.OAccountId, actionBase, 
								event_.ExecutionTime.Date, event_.ExecutionTime.AddHours(2).Date);

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
								if (actionsScheduledForTheNextHour.Count() >= ImageActionOptions.CreatePhotoActionHourlyLimit.Max)
								{
									event_.ExecutionTime.AddHours(1);
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
								if (actionsScheduledForTheNextHour.Count() >= VideoActionOptions.CreateVideoActionHourlyLimit.Max)
								{
									event_.ExecutionTime.AddHours(1);
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
								if (actionsScheduledForTheNextHour.Count() >= CommentingActionOptions.CommentingActionHourlyLimit.Max)
								{
									event_.ExecutionTime.AddHours(1);
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
								if (actionsScheduledForTheNextHour.Count() >= LikeActionOptions.LikeActionHourlyLimit.Max)
								{
									event_.ExecutionTime.AddHours(1);
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
								if (actionsScheduledForTheNextHour.Count() >= FollowActionOptions.FollowActionHourlyLimit.Max)
								{
									event_.ExecutionTime.AddHours(1);
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
		private ScheduleWindow GetTodaysScheduleWindow(string accountId)
		{
			return new ScheduleWindow
			{
				CreateImageActions = _timelineLogic.GetScheduledEventsForUserForActionByDate(accountId, ActionNames.CreatePhoto, DateTime.Today),
				CommentingActions = _timelineLogic.GetScheduledEventsForUserForActionByDate(accountId, ActionNames.Comment, DateTime.Today),
				CreateVideoActions = _timelineLogic.GetScheduledEventsForUserForActionByDate(accountId, ActionNames.CreateVideo, DateTime.Today),
				FollowUserActions = _timelineLogic.GetScheduledEventsForUserForActionByDate(accountId, ActionNames.FollowUser, DateTime.Today),
				LikeMediaActions = _timelineLogic.GetScheduledEventsForUserForActionByDate(accountId, ActionNames.LikeMedia, DateTime.Today)
			};
		}
		enum Day
		{
			Monday,
			Tuesday,
			Wednsday,
			Thursday,
			Friday,
			Saturday,
			Sunday
		}
		struct Time
		{
			public int Hours;
			public int Minutes;
			public int Seconds;
			public int Milliseconds;
		}
		struct TimelineScheduleWindow
		{
			public IEnumerable<object> Items;
			public Day Day { get; set; }
			public Time Time { get; set; }
		}
		/*
		 * H1 H2 H3 H4 H5 H6 H7 H8 H9 H10 H11 H12
		 M I F L  C      (BREAK)             ....
		 T
		 W
		 T
		 F
		 S
		 S
			 
		*/
		public async Task<AgentRespnse> StartAgent(string accountId, string instagramAccountId, string accessToken)
		{
			try
			{ 		
				var profile = await _profileLogic.GetProfile(accountId, instagramAccountId);
				if (profile == null) return null;
				_userStoreDetails.AddUpdateUser(accountId,instagramAccountId,accessToken);			
				_contentManager.SetUser(_userStoreDetails);
				var todays = _timelineLogic.GetAllEventsForUser(accountId, DateTime.Today, instaId:instagramAccountId, limit:9000);
				var groupby = todays.OrderBy(_=>_.Response.EnqueueTime).GroupBy(_=>_.TimelineType);
				
				var todaysImagesAction = _timelineLogic.GetAllEventsForUserByAction(ActionNames.LikeMedia,accountId,DateTime.Today,instaId:instagramAccountId,limit:10000);

				var imageAction = ActionsManager.Begin.Commit(ActionType.CreatePostTypeImage, _contentManager, profile, (UserStore) _userStoreDetails)
					.IncludeStrategy(new ImageStrategySettings
					{
						ImageStrategyType = ImageStrategyType.Default
					});
				var followAction = ActionsManager.Begin.Commit(ActionType.FollowUser, _contentManager, profile, (UserStore) _userStoreDetails)
					.IncludeStrategy(new FollowStrategySettings {
						FollowStrategy = FollowStrategyType.Default,
						NumberOfActions = 1,
						OffsetPerAction = DateTimeOffset.Now.AddSeconds(20)
					});
				var likeMediaAction = ActionsManager.Begin.Commit(ActionType.LikePost, _contentManager, profile, (UserStore)_userStoreDetails)
					.IncludeStrategy(new LikeStrategySettings
					{
						LikeStrategy = LikeStrategyType.Default,
						NumberOfActions = 9,
						OffsetPerAction = TimeSpan.FromSeconds(25)
					});
				var commentAction =  ActionsManager.Begin.Commit(ActionType.CreateCommentMedia, _contentManager, profile, (UserStore) _userStoreDetails)
					.IncludeStrategy(new CommentingStrategySettings
					{
						CommentingStrategy = CommentingStrategy.Default,
						NumberOfActions = 9,
						OffsetPerAction = TimeSpan.FromSeconds(25)
					});

				//every 2 days 
				// go through following list
				// check if any of the following people are engaging && and user's are 2-4 days old
				// if not then unfollow

				ScheduleWindow scheduleWindowForToday = GetTodaysScheduleWindow(accountId);
				List<Chance<ActionContainer>> ChanceAction = new List<Chance<ActionContainer>>();

				var imageOptions = new ImageActionOptions
				{
					ExecutionTime = scheduleWindowForToday.CreateImageActions.LastOrDefault() == null 
					? DateTime.UtcNow.AddMinutes(1) : scheduleWindowForToday.CreateImageActions.LastOrDefault().EnqueueTime.Value.AddMinutes(5),
					ImageFetchLimit = 20
				};
				var followOptions = new FollowActionOptions
				{
					FollowActionType = FollowActionType.Any,
					ExecutionTime = scheduleWindowForToday.FollowUserActions.LastOrDefault() == null
					? DateTime.UtcNow.AddMinutes(1) : scheduleWindowForToday.FollowUserActions.LastOrDefault().EnqueueTime.Value.AddMinutes(1),
				};
				var likeOptions = new LikeActionOptions
				{
					LikeActionType = LikeActionType.Any,
					ExecutionTime = scheduleWindowForToday.LikeMediaActions.LastOrDefault() == null
					? DateTime.UtcNow.AddMinutes(1) : scheduleWindowForToday.LikeMediaActions.LastOrDefault().EnqueueTime.Value.AddMinutes(1),
				};
				var commentingOptions = new CommentingActionOptions
				{
					CommentingActionType = CommentingActionType.Any,
					ExecutionTime = scheduleWindowForToday.CommentingActions.LastOrDefault() == null
					? DateTime.UtcNow.AddMinutes(1) : scheduleWindowForToday.CommentingActions.LastOrDefault().EnqueueTime.Value.AddMinutes(1),
				};

				/*ChanceAction.Add(new Chance<ActionContainer>
				{
					Object = new ActionContainer
					{
						ActionOptions = imageOptions,
						Action_ = (o) => RunAction(imageAction,o)
					},
					Probability = 0.10
				});*/
				
				/*ChanceAction.Add(new Chance<ActionContainer>
				{
					Object = new ActionContainer
					{
						ActionOptions = followOptions,
						Action_ = (o) => RunAction(followAction, o)
					},
					Probability = 0.15
				});*/
				/*
				ChanceAction.Add(new Chance<ActionContainer>
				{
					Object = new ActionContainer
					{
						ActionOptions = commentingOptions,
						Action_ = (o) => RunAction(commentAction, o)
					},
					Probability = 0.25
				});*/
				ChanceAction.Add(new Chance<ActionContainer>
				{
					Object = new ActionContainer
					{
						ActionOptions = likeOptions,
						Action_ = (o) => RunAction(likeMediaAction, o)
					},
					Probability = 1
				});

				System.Timers.Timer refreshTime = new System.Timers.Timer(TimeSpan.FromSeconds(2.5).TotalMilliseconds);
				refreshTime.Start();
				refreshTime.Elapsed += (o, e) =>
				{
					GetTodaysScheduleWindow(accountId);
					imageOptions = new ImageActionOptions
					{
						ExecutionTime = scheduleWindowForToday.CreateImageActions.LastOrDefault() == null ? 
						DateTime.UtcNow.AddMinutes(1) : scheduleWindowForToday.CreateImageActions.LastOrDefault().EnqueueTime.Value.AddMinutes(5),
						ImageFetchLimit = 20
					};
					followOptions = new FollowActionOptions
					{
						FollowActionType = FollowActionType.Any,
						ExecutionTime = scheduleWindowForToday.FollowUserActions.LastOrDefault() == null
						? DateTime.UtcNow.AddMinutes(1) : scheduleWindowForToday.FollowUserActions.LastOrDefault().EnqueueTime.Value.AddMinutes(1),
					};
					likeOptions = new LikeActionOptions
					{
						LikeActionType = LikeActionType.Any,
						ExecutionTime = scheduleWindowForToday.LikeMediaActions.LastOrDefault() == null
						? DateTime.UtcNow.AddMinutes(1) : scheduleWindowForToday.LikeMediaActions.LastOrDefault().EnqueueTime.Value.AddMinutes(1),
					};
					commentingOptions = new CommentingActionOptions
					{
						CommentingActionType = CommentingActionType.Any,
						ExecutionTime = scheduleWindowForToday.CommentingActions.LastOrDefault() == null
						? DateTime.UtcNow.AddMinutes(1) : scheduleWindowForToday.CommentingActions.LastOrDefault().EnqueueTime.Value.AddMinutes(1),
					};
				};

				_ = Task.Run(() =>
				{
					while (true)
					{

						var run_ = SecureRandom.ProbabilityRoll(ChanceAction);
						run_.Action_(run_.ActionOptions);
						Thread.Sleep(TimeSpan.FromSeconds(4));
					}
				});

				return new AgentRespnse()
				{
					HttpStatus = HttpStatusCode.OK,
					Message = "Started"
				};
			}
			catch(Exception ee)
			{
				return new AgentRespnse{
					HttpStatus = HttpStatusCode.InternalServerError,
					Message = ee.Message
				};
			}
		}
	}
}
