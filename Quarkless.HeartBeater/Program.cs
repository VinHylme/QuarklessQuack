using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quarkless.Common;
using Quarkless.HeartBeater.__Init__;
using Quarkless.Services.ContentBuilder.TopicBuilder;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Quarkless.HeartBeater.Creator;

namespace Quarkless.HeartBeater
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args.Length <= 0) return;
			var settingPath = Path.GetFullPath(Path.Combine(Accessors.BasePath, @"Quarkless"));
			IConfiguration configuration = new ConfigurationBuilder().
				SetBasePath(settingPath).AddJsonFile("appsettings.json").Build();

			var accessors = new Accessors(configuration);
			var services = new ServiceCollection();
			services.AddConfigurators(accessors);
			services.AddLogics();
			services.AddContexts();
			services.AddHandlers();
			services.AddRepositories();
			services.AddTransient<IInit,Init>();
			services.AddTransient<ITopicBuilder, TopicBuilder>();
			services.AddTransient<ICreator, Creator.Creator>();
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
						if (args.Length < 1) return;
						int.TryParse(args[1], out var typeAction);
						settings.ActionExecute = (ActionExecuteType) typeAction;
 						await servicePreacher.Get<IInit>().Endeavor(settings);
						break;
					case "app2":
						if(args.Length < 2) throw new Exception("Invalid Number of args");
						var corpusSettings = new CorpusSettings
						{
							Accounts = new List<Account> { new Account("lemonkaces") },
							PerformCleaning = bool.Parse(args[1])
						};
						await servicePreacher.Get<IInit>().Populator(corpusSettings);
						break;
					case "app3":
						await servicePreacher.Get<IInit>().Creator();
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
