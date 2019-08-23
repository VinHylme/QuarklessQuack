using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quarkless.Common;
using Quarkless.HeartBeater.__Init__;
using Quarkless.Services.ContentBuilder.TopicBuilder;
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
			if (args.Length <= 0) return;
			var settingPath = Path.GetFullPath(Path.Combine(@"C:\Users\yousef.alaw\source\repos\QuarklessQuark\Quarkless"));
			IConfiguration configuration = new ConfigurationBuilder().
				SetBasePath(settingPath).AddJsonFile("appsettings.json").Build();

			var accessors = new Accessors(configuration);
			var services = new ServiceCollection();
			services.AddLogics();
			services.AddContexts();
			services.AddHandlers();
			services.AddRepositories(accessors);
			services.AddTransient<IInit,Init>();
			services.AddTransient<ITopicBuilder, TopicBuilder>();
			services.AddLogging();
			var servicePreacher = new ServiceReacher(services.BuildServiceProvider());
			var settings = new Settings
			{
				Accounts = new List<Account> { new Account("lemonkaces") }
			};
			var results = WithExceptionLogAsync(async () =>
			{
				switch (args[0])
				{
					case "app1":
						await servicePreacher.Get<IInit>().Endeavor(settings);
						break;
					case "app2":
						await servicePreacher.Get<IInit>().Populator(settings);
						break;
					default:
						return;
				}
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
