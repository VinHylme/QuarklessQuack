using System;
using System.Threading.Tasks;
using InstagramApiSharp.Classes.Models;
using Quarkless.Base.Actions.Models;
using Quarkless.Base.Actions.Models.Interfaces;
using Quarkless.Base.Media.Models;
using Quarkless.Base.ResponseResolver.Models.Interfaces;
using Quarkless.Base.WorkerManager.Models.Interfaces;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Common.Models.Carriers;

namespace Quarkless.Base.Actions.Logic.Action_Executes
{
	internal class AccountCheckerExecute : IActionExecute
	{
		private readonly IWorker _worker;
		private readonly IResponseResolver _responseResolver;
		internal AccountCheckerExecute(IWorker worker, IResponseResolver responseResolver)
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

				if (!(eventAction.Body is DeleteMediaModel deleteMediaRequest))
				{
					result.IsSuccessful = false;
					result.Info = new ErrorResponse {Message = "Media Request is empty"};
					return result;
				}

				if (_worker.Client.GetContext.Container.InstagramAccount
					.BlockedActions.Exists(_ => _.ActionType == eventAction.ActionType))
				{
					result.IsSuccessful = false;
					result.Info = new ErrorResponse {Message = "Limit reached for this action"};
					return result;
				}

				var response = await _responseResolver
					.WithClient(_worker.Client)
					.WithAttempts(1)
					.WithResolverAsync(()=> _worker.Client
						.Media.DeleteMediaAsync(deleteMediaRequest.MediaId,
							(InstaMediaType) deleteMediaRequest.MediaType),
					ActionType.MaintainAccount, deleteMediaRequest);

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
				result.Results = response.Response.Value;
				return result;
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
