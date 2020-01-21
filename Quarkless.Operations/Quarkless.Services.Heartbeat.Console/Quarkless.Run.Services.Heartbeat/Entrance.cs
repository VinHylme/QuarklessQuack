using Microsoft.Extensions.DependencyInjection;
using Quarkless.Models.Services.Heartbeat.Enums;
using Quarkless.Models.Services.Heartbeat.Interfaces;
using System;
using System.Threading.Tasks;
using Quarkless.Models.Services.Heartbeat;
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

			using var scope = services.BuildServiceProvider().CreateScope();

			await WithExceptionLogAsync(async () 
				=> await scope.ServiceProvider.GetService<IHeartbeatService>()
					.Start(new CustomerAccount { UserId = userId, InstagramAccountId = instagramId }, operationType));
		}
	}
}
