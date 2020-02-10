using Microsoft.Extensions.DependencyInjection;
using Quarkless.Models.Services.Heartbeat.Enums;
using Quarkless.Models.Services.Heartbeat.Interfaces;
using System;
using System.Threading.Tasks;
using Quarkless.Logic.WorkerManager;
using Quarkless.Models.InstagramAccounts.Interfaces;
using Quarkless.Models.InstagramClient.Interfaces;
using Quarkless.Models.ResponseResolver.Interfaces;
using Quarkless.Models.Services.Heartbeat;
using Quarkless.Models.WorkerManager.Interfaces;
using Quarkless.Run.Services.Heartbeat.Extensions;
using Environment = System.Environment;

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
