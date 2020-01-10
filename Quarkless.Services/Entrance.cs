using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System;
using System.IO;
using System.Threading.Tasks;
using Quarkless.Common.Clients;
using QuarklessContexts.Models.SecurityLayerModels;

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

	public class Entrance
	{
		private const string CLIENT_SECTION = "Client";
		private static IConfiguration MakeConfigurationBuilder()
		{
			return new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory().Split("bin")[0])
				.AddJsonFile("appsettings.json").Build();
		}

		private static ServiceReacher InitialiseClientServices()
		{
			var cIn = new ClientRequester();
			if (!cIn.TryConnect().GetAwaiter().GetResult())
				throw new Exception("Invalid Client");

			var caller = MakeConfigurationBuilder().GetSection(CLIENT_SECTION).Get<AvailableClient>();

			var services = cIn.Build(caller, ServiceTypes.AddConfigurators,
				ServiceTypes.AddLogics,
				ServiceTypes.AddContexts,
				ServiceTypes.AddHandlers,
				ServiceTypes.AddAuthHandlers,
				ServiceTypes.AddRepositories,
				ServiceTypes.AddHangFrameworkServices,
				ServiceTypes.AddEventServices);

			if(services == null) 
				throw new Exception("Could not retrieve services");

			var endPoints = cIn.GetPublicEndPoints(new GetPublicEndpointCommandArgs());

			//cIn.TryDisconnect();
			var redis = ConnectionMultiplexer.Connect(endPoints.RedisCon);

			services.AddTransient<IAgentManager, AgentManager>();
			services.AddLogging();

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

		static void Main(string[] args)
		{
			var environmentVariables = Environment.GetEnvironmentVariables();
			var userId = environmentVariables["USER_ID"].ToString();
			var instagramId = environmentVariables["USER_INSTAGRAM_ACCOUNT"].ToString();

			var results = WithExceptionLogAsync(async () =>
			{
				await InitialiseClientServices().Get<IAgentManager>().Begin(userId, instagramId);
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
