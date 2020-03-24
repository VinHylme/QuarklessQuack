using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using NUnit.Framework;
using Quarkless.Base.Analytics.Logic;
using Quarkless.Base.Analytics.Models.Interfaces;
using Quarkless.Base.Auth.Common.Models.Interfaces;
using Quarkless.Base.AuthDetails.Logic;
using Quarkless.Base.ContentSearch.Logic;
using Quarkless.Base.ContentSearch.Models.Interfaces;
using Quarkless.Base.ContentSearch.Repository;
using Quarkless.Base.Hashtag.Logic;
using Quarkless.Base.Hashtag.Models.Interfaces;
using Quarkless.Base.Hashtag.Repository;
using Quarkless.Base.HashtagGenerator.Models;
using Quarkless.Base.HashtagGenerator.Models.Interfaces;
using Quarkless.Base.InstagramAccounts.Logic;
using Quarkless.Base.InstagramAccounts.Models.Interfaces;
using Quarkless.Base.InstagramAccounts.Repository.Mongo;
using Quarkless.Base.InstagramClient.Logic;
using Quarkless.Base.InstagramClient.Models.Interfaces;
using Quarkless.Base.Profile.Logic;
using Quarkless.Base.Profile.Models.Interfaces;
using Quarkless.Base.Profile.Repository;
using Quarkless.Base.Proxy.Logic;
using Quarkless.Base.Proxy.Models.Interfaces;
using Quarkless.Base.Proxy.Repository;
using Quarkless.Base.PuppeteerClient.Logic;
using Quarkless.Base.PuppeteerClient.Models.Interfaces;
using Quarkless.Base.ReportHandler.Logic;
using Quarkless.Base.ReportHandler.Models.Interfaces;
using Quarkless.Base.ReportHandler.Repository;
using Quarkless.Base.ResponseResolver.Models.Interfaces;
using Quarkless.Base.RestSharpClientManager.Logic;
using Quarkless.Base.RestSharpClientManager.Models.Interfaces;
using Quarkless.Base.Topic.Logic;
using Quarkless.Base.Topic.Models.Interfaces;
using Quarkless.Base.Topic.Repository;
using Quarkless.Base.WorkerManager.Logic;
using Quarkless.Base.WorkerManager.Models.Interfaces;
using Quarkless.Events;
using Quarkless.Events.Interfaces;
using Quarkless.Events.Models;
using Quarkless.Models.Shared.Extensions;
using Quarkless.Repository.MongoContext;
using Quarkless.Repository.MongoContext.Models;
using Quarkless.Repository.RedisContext;
using Quarkless.Repository.RedisContext.Models;
using Quarkless.Vision;

namespace Quarkless.Test.HashtagGenerator
{
	public class HashtagGeneratorIntegrationTests
	{
		private IHashtagGenerator _hashtagGenerator;
		private IHashtagsAnalytics _hashtagsAnalytics;
		[SetUp]
		public void Setup()
		{
			Environment.SetEnvironmentVariable("DOTNET_ENV_RELEASE", "dev");

			IServiceCollection services = new ServiceCollection();
			var accessors = new Config().Environments;

			services.AddSingleton<IHashtagsAnalytics, HashtagsAnalytics>();

			services.AddSingleton<IHashtagGenerator, Base.HashtagGenerator.Logic.HashtagGenerator>();
			services.AddSingleton<IHashtagLogic, HashtagLogic>();
			services.AddSingleton<IReportHandler, ReportHandler>();
			services.AddSingleton<ILoggerStore, LoggerStore>();
			services.AddSingleton<IReportHandlerRepository, ReportHandlerRepository>();
			services.AddSingleton<IHashtagsRepository, HashtagsRepository>();
			services.AddSingleton<IApiClientContainer, ApiClientContainer>();
			services.AddSingleton<IApiClientContext, ApiClientContext>();
			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
			services.AddSingleton<IUserContext, UserContext>();
			services.AddSingleton<IClientContextProvider, ClientContextProvider>();
			services.AddSingleton<IInstagramAccountLogic, InstagramAccountLogic>();
			services.AddSingleton<IProxyLogic, ProxyLogic>();
			services.AddSingleton<IProfileLogic, ProfileLogic>();
			services.AddSingleton<IInstagramAccountRepository, InstagramAccountRepository>();
			services.AddSingleton<IProfileRepository, ProfileRepository>();
			services.AddSingleton<IProxyAssignmentsRepository, ProxyAssignmentsRepository>();
			services.AddSingleton<ITopicLookupLogic, TopicLookupLogic>();
			services.AddSingleton<ITopicLookupRepository, TopicLookupRepository>();
			services.AddSingleton<IGoogleSearchLogic, GoogleSearchLogic>();
			services.AddSingleton<IRestSharpClientManager, RestSharpClientManager>();
			services.AddSingleton<IPuppeteerClient, PuppeteerClient>(s => new PuppeteerClient(1));
			services.AddSingleton<ISearchingCache, SearchingCache>();

			services.AddSingleton<IVisionClient, VisionClient>
				(s => new VisionClient(accessors.VisionCredentials, s.GetService<ISearchingCache>()));

			services.AddTransient<IEventSubscriber<InstagramAccountPublishEventModel>, ProfileLogic>();
			services.AddTransient<IEventSubscriber<ProfileTopicAddRequest>, TopicLookupLogic>();
			services.AddTransient<IEventPublisher, EventPublisher>(
				s => new EventPublisher(services));

			services.Configure<RedisOptions>(o =>
			{
				o.ConnectionString = accessors.RedisConnectionString;
				o.DefaultKeyExpiry = TimeSpan.FromDays(7);
			});

			services.AddSingleton<IWorkerManager, WorkerManager>
			(s => new WorkerManager(s.GetService<IApiClientContext>(),
				s.GetService<IInstagramAccountLogic>(),
				s.GetService<IResponseResolver>(), 1));

			services.AddTransient<IRedisClient, RedisClient>();

			BsonSerializer.RegisterSerializer(typeof(Guid), new GuidSerializer(BsonType.String));

			services.AddTransient<IMongoClientContext, MongoClientContext>(s =>
				new MongoClientContext(new MongoOptions
				{
					ConnectionString = accessors.ConnectionString,
					AccountCreatorDatabase = accessors.AccountCreationDatabase,
					AccountDatabase = accessors.MainDatabase,
					StatisticsDatabase = accessors.StatisticsDatabase,
					ControlDatabase = accessors.ControlDatabase,
					ContentDatabase = accessors.ContentDatabase
				}));

			using var scope = services.BuildServiceProvider().CreateScope();
			_hashtagGenerator = scope.ServiceProvider.GetService<IHashtagGenerator>();
			_hashtagsAnalytics = scope.ServiceProvider.GetService<IHashtagsAnalytics>();
		}

		[Test]
		public async Task SuggestHashtagFromImage()
		{
			var imageUrl = "https://i.pinimg.com/736x/2d/e7/38/2de7385640a11c47207e8ee9685de06a.jpg";
			var imageUrl2 = "https://рисуй.укр/images/detailed/2/GX23555.jpg";
			var results = await _hashtagGenerator.SuggestHashtags(new Source
			{
				ImageUrls = new[] {imageUrl2}
			},includeMediaExample:false);

			var tags = results.Results.Select(s => s.Name);
		}

		[Test]
		public async Task HashtagAnalysisTest()
		{
			var results = await _hashtagsAnalytics.GetHashtagAnalysis("psychedelicvideo");
		}

		[Test]
		public async Task SuggestHashtagNoSource()
		{
			var results = await _hashtagGenerator.SuggestHashtags(new Source());
			const bool failed = false;
			Assert.AreEqual(failed, results.IsSuccessful);
		}
	}
}
