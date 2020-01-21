using Microsoft.Extensions.DependencyInjection;
using Quarkless.Models.Services.Heartbeat.Enums;
using Quarkless.Models.Services.Heartbeat.Interfaces;
using System;
using System.Threading.Tasks;
using Quarkless.Models.Services.Heartbeat;
using Environment = System.Environment;
using Quarkless.Logic.Services.Heartbeat;

namespace Quarkless.Run.Services.Heartbeat
{
	class Entrance
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

			services.AddSingleton<IHeartbeatService, HeartbeatService>();

			var buildService = services.BuildServiceProvider();

			await WithExceptionLogAsync(async () => await buildService.GetService<IHeartbeatService>()
				.Start(new CustomerAccount{UserId = userId, InstagramAccountId =  instagramId}, operationType));
		}
	}
}
