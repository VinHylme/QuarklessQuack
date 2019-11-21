using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quarkless.Services.ContentBuilder.TopicBuilder;
using Quarkless.Services.Interfaces;
#region Used when initially populating corpus
using QuarklessRepositories.RedisRepository.CorpusCache.CommentCorpusCache;
using QuarklessRepositories.RedisRepository.CorpusCache.HashtagCorpusCache;
using QuarklessRepositories.RedisRepository.CorpusCache.MediaCorpusCache;
using QuarklessRepositories.Repository.CorpusRepositories.Comments;
using QuarklessRepositories.Repository.CorpusRepositories.Medias;
using QuarklessRepositories.Repository.ServicesRepositories.CaptionsRepository;
using QuarklessRepositories.Repository.ServicesRepositories.CommentsRepository;
using QuarklessRepositories.Repository.ServicesRepositories.HashtagsRepository;
#endregion
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

			var validate = cIn.Send(new InitCommandArgs
			{
				Client = caller,
				CommandName = "Validate Client"
			});
			if (!(bool)validate)
				throw new Exception("Could not validated");

			var services = (IServiceCollection) cIn.Send(new BuildCommandArgs()
			{
				CommandName = "Build Services",
				ServiceTypes = new[]
				{
					ServiceTypes.AddConfigurators,
					ServiceTypes.AddLogics,
					ServiceTypes.AddContexts,
					ServiceTypes.AddHandlers,
					ServiceTypes.AddAuthHandlers,
					ServiceTypes.AddRepositories,
					ServiceTypes.AddHangFrameworkServices
				}
			});
			var endPoints = (EndPoints) cIn.Send(new GetPublicEndpointCommandArgs
			{
				CommandName = "Get Public Endpoints",
			});

			var redis = ConnectionMultiplexer.Connect(endPoints.RedisCon);

			services.AddTransient<IAgentManager, AgentManager>();
			services.AddTransient<ITopicBuilder, TopicBuilder>();
			services.AddSingleton<IContentManager, ContentManager>();
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
			var results = WithExceptionLogAsync(async () =>
			{
				//lunch
				//do this when needing to populate table in cache

				//var cT = Task.Run(async () => { 
				//	var c = await serviceReacher.Get<ICommentCorpusRepository>().GetComments();
				//	await serviceReacher.Get<ICommentCorpusCache>().AddComments(c);
				//});
				//var ccT = Task.Run(async () => { 
				//	var cc = await serviceReacher.Get<IMediaCorpusRepository>().GetMedias();
				//	await serviceReacher.Get<IMediaCorpusCache>().AddMedias(cc);
				//});
				//var hT = Task.Run(async () => { 
				//	var h = await serviceReacher.Get<IHashtagsRepository>().GetHashtags();
				//	await serviceReacher.Get<IHashtagCoprusCache>().AddHashtags(h);
				//});
				//Task.WaitAll(cT, ccT, hT);

				await InitialiseClientServices().Get<IAgentManager>().Begin();
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
