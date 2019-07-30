using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quarkless.Common;
using Quarkless.Services.ContentBuilder.TopicBuilder;
using Quarkless.Services.Interfaces;
using StackExchange.Redis;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Quarkless.Services
{
	public class WorkerActivator : JobActivator
	{
		private readonly IServiceProvider _serviceProvider;

		public WorkerActivator(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}
		public override object ActivateJob(Type jobType) => _serviceProvider.GetService(jobType);
	}
	public class ServiceReacher
	{
		private readonly IServiceProvider _serviceProvider;
		public ServiceReacher(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}
		public TInstance Get<TInstance>() => _serviceProvider.GetService<TInstance>();
	}

	public class Enterance
	{
		static void Main(string[] args)
		{
			var settingPath = Path.GetFullPath(Path.Combine(@"C:\Users\yousef.alaw\source\repos\QuarklessQuark\Quarkless"));
			IConfiguration configuration = new ConfigurationBuilder().
				SetBasePath(settingPath).AddJsonFile("appsettings.json").Build();
			Accessors accessors = new Accessors(configuration);
			
			var Redis = ConnectionMultiplexer.Connect(accessors.RedisConnectionString);
			var services = new ServiceCollection();
			
			services.AddLogics();
			services.AddContexts();
			services.AddHandlers();
			services.AddAuthHandlers(accessors, configuration.GetAWSOptions());
			services.AddRepositories(accessors);

			services.AddTransient<IAgentManager, AgentManager>();
			services.AddTransient<ITopicBuilder, TopicBuilder>();
			services.AddSingleton<IContentManager, ContentManager>();
			services.AddLogging();
			services.AddHangFrameworkServices(accessors);

			GlobalConfiguration.Configuration.UseRedisStorage(Redis, new Hangfire.Redis.RedisStorageOptions
			{
				Db = 1,
				Prefix = "Timeline",
				SucceededListSize = 5000,
				DeletedListSize = 1000,
				ExpiryCheckInterval = TimeSpan.FromHours(1),
				InvisibilityTimeout = TimeSpan.FromMinutes(30),
				FetchTimeout = TimeSpan.FromMinutes(30),
				UseTransactions = true
			}).WithJobExpirationTimeout(TimeSpan.FromHours(12));

			ServiceReacher serviceReacher = new ServiceReacher(services.BuildServiceProvider());

			var results = WithExceptionLogAsync(async () =>
			{
				//lunch
				await serviceReacher.Get<IAgentManager>().Begin();
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
