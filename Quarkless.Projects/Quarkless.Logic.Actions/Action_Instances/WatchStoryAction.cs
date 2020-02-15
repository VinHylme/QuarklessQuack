using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Quarkless.Models.Actions;
using Quarkless.Models.Actions.Enums.ActionTypes;
using Quarkless.Models.Actions.Enums.StrategyType;
using Quarkless.Models.Actions.Factory.Action_Options;
using Quarkless.Models.Actions.Interfaces;
using Quarkless.Models.Actions.Models;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Common.Extensions;
using Quarkless.Models.Common.Models;
using Quarkless.Models.Common.Models.Carriers;
using Quarkless.Models.ContentInfo.Interfaces;
using Quarkless.Models.Heartbeat.Interfaces;
using Quarkless.Models.Stories;

namespace Quarkless.Logic.Actions.Action_Instances
{
	internal class WatchStoryAction : StoryActionFetcher, IActionCommit
	{
		private readonly UserStoreDetails _user;
		private WatchStoryOptions _actionOptions;
		internal WatchStoryAction(UserStoreDetails userStoreDetails, IContentInfoBuilder contentInfoBuilder,
			IHeartbeatLogic heartbeatLogic) : base(heartbeatLogic, userStoreDetails)
		{
			_user = userStoreDetails;
			_actionOptions = new WatchStoryOptions();
		}


		public async Task<ResultCarrier<EventActionModel>> PushAsync(DateTimeOffset executionTime)
		{
			Console.WriteLine($"Watch Story Action Started: {_user.AccountId}, {_user.InstagramAccountUsername}, {_user.InstagramAccountUser}");
			var results = new ResultCarrier<EventActionModel>();
			try
			{
				switch (_actionOptions.StrategySettings.StrategyType)
				{
					case WatchStoryStrategyType.Default: // watch a single users story
					{
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
								Message = $"could not find any stories to watch, user: {_user.AccountId}, instaId: {_user.InstagramAccountUsername}",
								StatusCode = System.Net.HttpStatusCode.NotFound
							};
							return results;
						}

						var @event = new EventActionModel($"WatchStory_{_actionOptions.StrategySettings.StrategyType.ToString()}_{storyActionType.ToString()}")
						{
							ActionType = ActionType.WatchStory,
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
					}
					case WatchStoryStrategyType.MultipleUsers: // watch multiple users story
					{
						throw new NotImplementedException();
					}
					default:throw new ArgumentOutOfRangeException();
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
				Console.WriteLine($"Watch Story Action Ended: {_user.AccountId}, {_user.InstagramAccountUsername}, {_user.InstagramAccountUser}");
			}
		}

		public IActionCommit ModifyOptions(IActionOptions newOptions)
		{
			try
			{
				_actionOptions = newOptions as WatchStoryOptions;
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
			}
			return this;
		}
	}
}