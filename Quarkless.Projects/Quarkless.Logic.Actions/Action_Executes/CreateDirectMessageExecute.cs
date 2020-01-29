using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Quarkless.Models.Actions;
using Quarkless.Models.Actions.Interfaces;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Common.Extensions;
using Quarkless.Models.Common.Models.Carriers;
using Quarkless.Models.Messaging;
using Quarkless.Models.ResponseResolver.Interfaces;
using Quarkless.Models.WorkerManager.Interfaces;

namespace Quarkless.Logic.Actions.Action_Executes
{
	internal class CreateDirectMessageExecute: IActionExecute
	{
		private readonly IWorker _worker;
		private readonly IResponseResolver _responseResolver;
		internal CreateDirectMessageExecute(IWorker worker, IResponseResolver responseResolver)
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

				if (eventAction.BodyType == typeof(SendDirectTextModel))
				{
					var model = JsonConvert.DeserializeObject<SendDirectTextModel>(eventAction.Body.ToJsonString());
					var recipients = string.Empty;
					var threads = string.Empty;

					if (model.Recipients != null && model.Recipients.Any())
						recipients = string.Join(",", model.Recipients);

					if (model.Threads != null && model.Threads.Any())
						threads = string.Join(",", model.Threads);

					var response = await _responseResolver.WithClient(_worker.Client)
						.WithResolverAsync(
							await _worker.Client.Messaging.SendDirectTextAsync(recipients, threads,
								model.TextMessage), ActionType.SendDirectMessageText,
							model.ToJsonString());

					if (!response.Succeeded)
					{
						result.IsSuccessful = false;
						result.Info = new ErrorResponse
						{
							Message = response.Info.Message,
							Exception = response.Info.Exception
						};
						return result;
					}

					result.IsSuccessful = true;
					result.Results = true;
					return result;
				}
				else if (eventAction.BodyType == typeof(SendDirectLinkModel))
				{
					var model = JsonConvert.DeserializeObject<SendDirectLinkModel>(eventAction.Body.ToJsonString());
					var response = await _responseResolver.WithClient(_worker.Client)
						.WithResolverAsync(await _worker.Client.Messaging.SendDirectLinkAsync(model.TextMessage,
								model.Link,
								model.Threads.ToArray(), model.Recipients.ToArray()),
							ActionType.SendDirectMessageLink,
							model.ToJsonString());
					if (!response.Succeeded)
					{
						result.IsSuccessful = false;
						result.Info = new ErrorResponse
						{
							Message = response.Info.Message,
							Exception = response.Info.Exception
						};
						return result;
					}

					result.IsSuccessful = true;
					result.Results = true;
					return result;
				}
				else if (eventAction.BodyType == typeof(SendDirectProfileModel))
				{
					var model = JsonConvert.DeserializeObject<SendDirectProfileModel>(eventAction.Body.ToJsonString());
					var recipients = string.Empty;
					if (model.Recipients != null && model.Recipients.Any())
						recipients = string.Join(",", model.Recipients);
					var response = await _responseResolver.WithClient(_worker.Client)
						.WithResolverAsync(
							await _worker.Client.Messaging.SendDirectProfileToRecipientsAsync(model.userId,
								recipients), ActionType.SendDirectMessageProfile, model.ToJsonString());

					if (!response.Succeeded)
					{
						result.IsSuccessful = false;
						result.Info = new ErrorResponse
						{
							Message = response.Info.Message,
							Exception = response.Info.Exception
						};
						return result;
					}

					result.IsSuccessful = true;
					result.Results = true;
					return result;
				}
				else if (eventAction.BodyType == typeof(SendDirectPhotoModel))
				{
					var model = JsonConvert.DeserializeObject<SendDirectPhotoModel>(eventAction.Body.ToJsonString());
					var response = await _responseResolver.WithClient(_worker.Client)
						.WithResolverAsync(
							await _worker.Client.Messaging.SendDirectPhotoToRecipientsAsync(model.Image,
								model.Recipients.ToArray()), ActionType.SendDirectMessagePhoto, model.ToJsonString());

					if (!response.Succeeded)
					{
						result.IsSuccessful = false;
						result.Info = new ErrorResponse
						{
							Message = response.Info.Message,
							Exception = response.Info.Exception
						};
						return result;
					}

					result.IsSuccessful = true;
					result.Results = true;
					return result;
				}
				else if (eventAction.BodyType == typeof(SendDirectVideoModel))
				{
					var model = JsonConvert.DeserializeObject<SendDirectVideoModel>(eventAction.Body.ToJsonString());
					var response = await _responseResolver.WithClient(_worker.Client)
						.WithResolverAsync(
							await _worker.Client.Messaging.SendDirectVideoToRecipientsAsync(model.Video,
								model.Recipients.ToArray()), ActionType.SendDirectMessageVideo, model.ToJsonString());

					if (!response.Succeeded)
					{
						result.IsSuccessful = false;
						result.Info = new ErrorResponse
						{
							Message = response.Info.Message,
							Exception = response.Info.Exception
						};
						return result;
					}

					result.IsSuccessful = true;
					result.Results = true;
					return result;
				}
				else
				{
					throw new Exception("Invalid Type");
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
