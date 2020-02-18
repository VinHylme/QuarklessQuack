using System;
using System.Linq;
using System.Threading.Tasks;
using MoreLinq.Extensions;
using Newtonsoft.Json;
using Quarkless.Models.Actions;
using Quarkless.Models.Actions.Interfaces;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Common.Extensions;
using Quarkless.Models.Common.Models.Carriers;
using Quarkless.Models.ResponseResolver.Interfaces;
using Quarkless.Models.Stories;
using Quarkless.Models.WorkerManager.Interfaces;

namespace Quarkless.Logic.Actions.Action_Executes
{
	internal class ReactStoryExecute : IActionExecute
	{
		private readonly IWorker _worker;
		private readonly IResponseResolver _responseResolver;

		internal ReactStoryExecute(IWorker worker, IResponseResolver responseResolver)
		{
			_worker = worker;
			_responseResolver = responseResolver;
		}
		public async Task<ResultCarrier<bool>> ExecuteAsync(EventExecuteBody eventAction)
		{
			var result = new ResultCarrier<bool>();
			try
			{
				Console.WriteLine($"Started to execute {GetType().Name} for {_worker.WorkerAccountId}/{_worker.WorkerUsername}");

				if (_worker.Client.GetContext.Container.InstagramAccount
					.BlockedActions.Exists(_ => _.ActionType == eventAction.ActionType))
				{
					result.IsSuccessful = false;
					result.Info = new ErrorResponse { Message = "Limit reached for this action" };
					return result;
				}

				var request = JsonConvert.DeserializeObject<StoryRequest>(eventAction.Body.ToJsonString());

				if (request.ContainsItems)
				{
					var response = await _responseResolver
						.WithClient(_worker.Client)
						.WithAttempts(1)
						.WithResolverAsync(() => _worker.Client.Story
								.SendReactionToStoryAsync(request.UserId,
									request.Items.Shuffle().First().StoryMediaId,
									request.Reaction),
							ActionType.ReactStory, request);

					if (!response.Response.Succeeded)
					{
						result.IsSuccessful = false;
						result.Info = new ErrorResponse
						{
							Message = response.Response.Info.Message,
							Exception = response.Response.Info.Exception
						};
						return result;
					}

					result.IsSuccessful = true;
					result.Results = response.Response.Succeeded;
					return result;
				}
				else
				{
					var storyOfUser = await _responseResolver
						.WithClient(_worker.Client)
						.WithAttempts(1)
						.WithResolverAsync(() => _worker.Client.Story.GetUserStoryAsync(request.UserId));

					if (!storyOfUser.Response.Succeeded || storyOfUser.Response.Value.Items.Count <= 0)
					{
						result.IsSuccessful = false;
						result.Info = new ErrorResponse
						{
							Message = storyOfUser.Response?.Info?.Message ?? "No Stories Available",
							Exception = storyOfUser.Response?.Info?.Exception
						};
						return result;
					}

					var response = await _responseResolver
						.WithClient(_worker.Client)
						.WithAttempts(1)
						.WithResolverAsync(() => _worker.Client.Story
								.SendReactionToStoryAsync(request.UserId,
									storyOfUser.Response.Value.Items.Shuffle().First().Id, request.Reaction),
							ActionType.ReactStory, request);

					if (!response.Response.Succeeded)
					{
						result.IsSuccessful = false;
						result.Info = new ErrorResponse
						{
							Message = response.Response.Info.Message,
							Exception = response.Response.Info.Exception
						};
						return result;
					}

					result.IsSuccessful = true;
					result.Results = response.Response.Succeeded;
					return result;
				}
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
				result.IsSuccessful = false;
				result.Info = new ErrorResponse
				{
					Message = err.Message,
					Exception = err
				};
				return result;
			}
			finally
			{
				Console.WriteLine($"Ended execute {GetType().Name} for {_worker.WorkerAccountId}/{_worker.WorkerUsername} Was Successful: {result.IsSuccessful}");
			}
		}
	}
}