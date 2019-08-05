using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quarkless.Common;
using Quarkless.Services.ContentBuilder.TopicBuilder;
using Quarkless.Services.Interfaces;
using QuarklessRepositories.RedisRepository.CorpusCache.CommentCorpusCache;
using QuarklessRepositories.RedisRepository.CorpusCache.HashtagCorpusCache;
using QuarklessRepositories.RedisRepository.CorpusCache.MediaCorpusCache;
using QuarklessRepositories.Repository.CorpusRepositories.Comments;
using QuarklessRepositories.Repository.CorpusRepositories.Medias;
using QuarklessRepositories.Repository.ServicesRepositories.CaptionsRepository;
using QuarklessRepositories.Repository.ServicesRepositories.CommentsRepository;
using QuarklessRepositories.Repository.ServicesRepositories.HashtagsRepository;
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
