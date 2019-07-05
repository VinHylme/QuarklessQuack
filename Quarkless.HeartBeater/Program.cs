using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quarkless.Common;
using Quarkless.HeartBeater.__Init__;
using Quarkless.HeartBeater.__MetadataBuilder__;
using QuarklessLogic.ServicesLogic.HeartbeatLogic;
using QuarklessRepositories.RedisRepository.HeartBeaterRedis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Quarkless.HeartBeater
{
	class Program
	{
		static void Main(string[] args)
		{
			var settingPath = Path.GetFullPath(Path.Combine(@"C:\Users\yousef.alaw\source\repos\QuarklessQuark\Quarkless"));
			IConfiguration configuration = new ConfigurationBuilder().
				SetBasePath(settingPath).AddJsonFile("appsettings.json").Build();

			Accessors accessors = new Accessors(configuration);
			var services = new ServiceCollection();
			services.AddLogics();
			services.AddContexts();
			services.AddHandlers();
			services.AddRepositories(accessors);
			services.AddTransient<IInit,Init>();
			ServiceReacher serviceReacher = new ServiceReacher(services.BuildServiceProvider());
			var results = WithExceptionLogAsync(async () =>
			{
				Settings settings = new Settings
				{
					Accounts = new List<Account> { new Account("lemonkaces") }
				};
				await serviceReacher.Get<IInit>().Endeavor(settings);
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
