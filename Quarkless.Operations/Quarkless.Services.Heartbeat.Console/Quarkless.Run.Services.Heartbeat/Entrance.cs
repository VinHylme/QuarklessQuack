using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Quarkless.Base.InstagramAccounts.Models.Interfaces;
using Quarkless.Base.InstagramClient.Models.Interfaces;
using Quarkless.Base.ResponseResolver.Models.Interfaces;
using Quarkless.Base.WorkerManager.Logic;
using Quarkless.Base.WorkerManager.Models.Interfaces;
using Quarkless.Run.Services.Heartbeat.Models.Extensions;
using CustomerAccount = Quarkless.Run.Services.Heartbeat.Models.CustomerAccount;
using Environment = System.Environment;
using ExtractOperationType = Quarkless.Run.Services.Heartbeat.Models.Enums.ExtractOperationType;
using IHeartbeatService = Quarkless.Run.Services.Heartbeat.Models.Interfaces.IHeartbeatService;

namespace Quarkless.Run.Services.Heartbeat
{
	internal class Entrance
	{
		private static Task WithExceptionLogAsync(Func<Task> actionAsync)
		{
			try
			{
				return actionAsync();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"{ex.Message}");
				throw;
			}
		}
		
		static async Task Main(string[] args)
		{
			var environmentVariables = Environment.GetEnvironmentVariables();

			var userId = environmentVariables["USER_ID"].ToString();
			var instagramId = environmentVariables["USER_INSTAGRAM_ACCOUNT"].ToString();
			var workerType = int.Parse(environmentVariables["USER_WORKER_ACCOUNT_TYPE"].ToString());
			var operationType = (ExtractOperationType) int.Parse(environmentVariables["OPERATION_TYPE"].ToString());

			if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(instagramId))
				return;

			IServiceCollection services = new ServiceCollection();
			
			services.IncludeLogicServices();
			services.IncludeContexts();
			services.IncludeConfigurators();
			services.IncludeRepositories();
			services.IncludeHandlers();
			services.IncludeEventServices();
			
			services.AddSingleton<IWorkerManager, WorkerManager>
			(s => new WorkerManager(s.GetService<IApiClientContext>(),
				s.GetService<IInstagramAccountLogic>(),
				s.GetService<IResponseResolver>(), 2, workerType));
			
			using var scope = services.BuildServiceProvider().CreateScope();

			await WithExceptionLogAsync(async () 
				=> await scope.ServiceProvider.GetService<IHeartbeatService>()
					.Start(new CustomerAccount { UserId = userId, InstagramAccountId = instagramId }, operationType));
		}
	}
}
