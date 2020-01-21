using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Quarkless.Models.Services.Automation.Interfaces;
using Quarkless.Run.Services.Automation.Extensions;

namespace Quarkless.Run.Services.Automation
{
	public class Entrance
	{
		static void Main(string[] args)
		{
			var environmentVariables = Environment.GetEnvironmentVariables();
			var userId = environmentVariables["USER_ID"].ToString();
			var instagramId = environmentVariables["USER_INSTAGRAM_ACCOUNT"].ToString();
			
			var services = new ServiceCollection();

			services.IncludeHangFrameworkServices();
			services.IncludeLogicServices();
			services.IncludeRepositories();
			services.IncludeAuthHandlers();
			services.IncludeConfigurators();
			services.IncludeContexts();
			services.IncludeEventServices();
			services.IncludeHandlers();
			
			var results = WithExceptionLogAsync(async () =>
			{
				using var scope = services.BuildServiceProvider().CreateScope();
				await scope.ServiceProvider.GetService<IAgentManager>().Begin(userId, instagramId);
			});

			Task.WaitAll(results);
		}

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
	}
}
