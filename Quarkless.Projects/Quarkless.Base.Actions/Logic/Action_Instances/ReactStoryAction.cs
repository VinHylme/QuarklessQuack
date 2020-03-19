using System;
using System.Threading.Tasks;
using Quarkless.Base.Actions.Models;
using Quarkless.Base.Actions.Models.Enums.ActionTypes;
using Quarkless.Base.Actions.Models.Enums.StrategyType;
using Quarkless.Base.Actions.Models.Factory.Action_Options;
using Quarkless.Base.Actions.Models.Interfaces;
using Quarkless.Base.ContentInfo.Models.Interfaces;
using Quarkless.Base.Heartbeat.Models.Interfaces;
using Quarkless.Base.Lookup.Models.Interfaces;
using Quarkless.Base.Stories.Models;
using Quarkless.Common.Timeline.Models;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Common.Models;
using Quarkless.Models.Common.Models.Carriers;

namespace Quarkless.Base.Actions.Logic.Action_Instances
{
	internal class ReactStoryAction : StoryActionFetcher, IActionCommit
	{
		private readonly UserStoreDetails _user;
		private ReactStoryOptions _actionOptions;
		private readonly IContentInfoBuilder _contentInfoBuilder;
		public ReactStoryAction(UserStoreDetails user, IContentInfoBuilder contentInfoBuilder,
			IHeartbeatLogic heartbeatLogic, ILookupLogic lookupLogic) 
			: base(heartbeatLogic, lookupLogic, ActionType.ReactStory, user)
		{
			_user = user;
			_contentInfoBuilder = contentInfoBuilder;
			_actionOptions = new ReactStoryOptions();
		}

		public async Task<ResultCarrier<EventActionModel>> PushAsync(DateTimeOffset executionTime)
		{
			Console.WriteLine($"React to a Story Action Started: {_user.AccountId}, {_user.InstagramAccountUsername}, {_user.InstagramAccountUser}");
			var results = new ResultCarrier<EventActionModel>();
			try
			{
				switch (_actionOptions.StrategySettings.StrategyType)
				{
					case ReactStoryStrategyType.Default:
						StoryRequest storyRequest;
						var storyActionType = GetStoryActionType(_actionOptions.storyActionType);
						switch (storyActionType)
						{
							case StoryActionType.WatchFromFeed:
								storyRequest = await GetStoryFromFeed();
								break;
							case StoryActionType.WatchByTopic:
								storyRequest = await GetStoryFromTopic();
								break;
							case StoryActionType.WatchBasedOnUserFollowers:
								storyRequest = await GetStoryFromFollowers();
								break;
							case StoryActionType.WatchFromLocationTarget:
								storyRequest = await GetStoryFromLocationTarget();
								break;
							case StoryActionType.WatchBasedOnSuggestedUsers:
								storyRequest = await GetStoryFromSuggestions();
								break;
							case StoryActionType.WatchFromLikes:
								storyRequest = await GetStoryFromLikers();
								break;
							case StoryActionType.WatchFromComments:
								storyRequest = await GetStoryFromCommenters();
								break;
							default:
								throw new ArgumentOutOfRangeException();
						}

						if (storyRequest == null)
						{
							results.IsSuccessful = false;
							results.Info = new ErrorResponse
							{
								Message = $"could not find any stories to react, user: {_user.AccountId}, instaId: {_user.InstagramAccountUsername}",
								StatusCode = System.Net.HttpStatusCode.NotFound
							};
							return results;
						}

						storyRequest.Reaction = _contentInfoBuilder.GenerateEmoji();

						var @event = new EventActionModel($"ReactStory_{_actionOptions.StrategySettings.StrategyType.ToString()}_{storyActionType.ToString()}")
						{
							ActionType = ActionType.ReactStory,
							User = new UserStore
							{
								AccountId = _user.AccountId,
								InstagramAccountUsername = _user.InstagramAccountUsername,
								InstagramAccountUser = _user.InstagramAccountUser
							}
						};

						@event.DataObjects.Add(new EventBody(storyRequest, storyRequest.GetType(), executionTime));
						results.IsSuccessful = true;
						results.Results = @event;
						return results;
					default: throw new ArgumentOutOfRangeException();
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
				Console.WriteLine(
					$"React to a Story Action Ended: {_user.AccountId}, {_user.InstagramAccountUsername}, {_user.InstagramAccountUser}");
			}
		}

		public IActionCommit ModifyOptions(IActionOptions newOptions)
		{
			try
			{
				_actionOptions = newOptions as ReactStoryOptions;
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
			}
			return this;
		}
	}
}