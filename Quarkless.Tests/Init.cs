using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quarkless.Common;
using Quarkless.Services;
using Quarkless.Services.ContentBuilder.TopicBuilder;
using Quarkless.Services.Interfaces;
using StackExchange.Redis;

namespace Quarkless.Tests
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
	public class Init
	{
		public Init()
		{

		}

		public ServiceReacher InitialiseContext()
		{
			var settingPath = Path.GetFullPath(Path.Combine(Accessors.BasePath, @"Quarkless"));

			IConfiguration configuration = new ConfigurationBuilder().
				SetBasePath(settingPath).AddJsonFile("appsettings.json").Build();
			var accessors = new Accessors(configuration);
			
			var redis = ConnectionMultiplexer.Connect(accessors.RedisConnectionString);
			var services = new ServiceCollection();

			services.AddConfigurators(accessors);
			services.AddLogics();
			services.AddContexts();
			services.AddHandlers();
			services.AddAuthHandlers(accessors, configuration.GetAWSOptions());
			services.AddRepositories();

			services.AddTransient<IAgentManager, AgentManager>();
			services.AddTransient<ITopicBuilder, TopicBuilder>();
			services.AddSingleton<IContentManager, ContentManager>();
			services.AddLogging();
			services.AddHangFrameworkServices(accessors);
			GlobalConfiguration.Configuration.UseRedisStorage(redis, new Hangfire.Redis.RedisStorageOptions
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
			return new ServiceReacher(services.BuildServiceProvider());
		}
	}
}
