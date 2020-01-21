using Hangfire;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Quarkless.Models.Services.Automation.Interfaces;
using Quarkless.Logic.Services.Automation;

namespace Quarkless.Run.Services.Automation
{
	public class ServiceReacher
	{
		private readonly IServiceProvider _serviceProvider;
		public ServiceReacher(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}
		public TInstance Get<TInstance>() => _serviceProvider.GetService<TInstance>();
	}

	public class Entrance
	{
		static void Main(string[] args)
		{
			var environmentVariables = Environment.GetEnvironmentVariables();
			var userId = environmentVariables["USER_ID"].ToString();
			var instagramId = environmentVariables["USER_INSTAGRAM_ACCOUNT"].ToString();

			// var results = WithExceptionLogAsync(async () =>
			// {
			// 	await InitialiseClientServices().Get<IAgentManager>().Begin(userId, instagramId);
			// });
			//Task.WaitAll(results);
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
