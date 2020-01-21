using System;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Quarkless.Base.AccountOptions;
using Quarkless.Base.ContentSearch;
using Quarkless.Base.InstagramComments;
using Quarkless.Events;
using Quarkless.Events.Interfaces;
using Quarkless.Logic.ContentSearch;
using Quarkless.Logic.Hashtag;
using Quarkless.Logic.Heartbeat;
using Quarkless.Logic.InstagramAccounts;
using Quarkless.Logic.InstagramClient;
using Quarkless.Logic.Lookup;
using Quarkless.Logic.Profile;
using Quarkless.Logic.Proxy;
using Quarkless.Logic.ReportHandler;
using Quarkless.Logic.ResponseResolver;
using Quarkless.Logic.RestSharpClientManager;
using Quarkless.Logic.SeleniumClient;
using Quarkless.Logic.Services.Heartbeat;
using Quarkless.Logic.Timeline;
using Quarkless.Logic.Topic;
using Quarkless.Logic.WorkerManager;
using Quarkless.Models.Auth;
using Quarkless.Models.Auth.Interfaces;
using Quarkless.Models.ContentSearch.Interfaces;
using Quarkless.Models.ContentSearch.Models;
using Quarkless.Models.Hashtag.Interfaces;
using Quarkless.Models.Heartbeat.Interfaces;
using Quarkless.Models.InstagramAccounts;
using Quarkless.Models.InstagramAccounts.Interfaces;
using Quarkless.Models.InstagramClient.Interfaces;
using Quarkless.Models.Lookup.Interfaces;
using Quarkless.Models.Profile;
using Quarkless.Models.Profile.Interfaces;
using Quarkless.Models.Proxy.Interfaces;
using Quarkless.Models.ReportHandler.Interfaces;
using Quarkless.Models.ResponseResolver.Interfaces;
using Quarkless.Models.RestSharpClientManager.Interfaces;
using Quarkless.Models.SeleniumClient;
using Quarkless.Models.SeleniumClient.Interfaces;
using Quarkless.Models.Services.Heartbeat.Interfaces;
using Quarkless.Models.Shared.Extensions;
using Quarkless.Models.Timeline.Interfaces;
using Quarkless.Models.Topic.Interfaces;
using Quarkless.Models.WorkerManager.Interfaces;
using Quarkless.Repository.Auth;
using Quarkless.Repository.ContentSearch;
using Quarkless.Repository.Hashtag;
using Quarkless.Repository.Heartbeat;
using Quarkless.Repository.InstagramAccounts.Mongo;
using Quarkless.Repository.InstagramAccounts.Redis;
using Quarkless.Repository.Lookup;
using Quarkless.Repository.MongoContext;
using Quarkless.Repository.MongoContext.Models;
using Quarkless.Repository.Profile;
using Quarkless.Repository.Proxy;
using Quarkless.Repository.RedisContext;
using Quarkless.Repository.RedisContext.Models;
using Quarkless.Repository.ReportHandler;
using Quarkless.Repository.Timeline;
using Quarkless.Repository.Topic;

namespace Quarkless.Run.Services.Heartbeat.Extensions
{
	public static class ConfigurationServices
	{
		internal static void IncludeLogicServices(this IServiceCollection services)
		{
			services.AddTransient<IProxyLogic, ProxyLogic>();
			services.AddTransient<IInstagramAccountLogic, InstagramAccountLogic>();
			services.AddTransient<IProfileLogic, ProfileLogic>();
			services.AddTransient<ICommentLogic, CommentLogic>();
			services.AddTransient<IInstaAccountOptionsLogic, InstaAccountOptionsLogic>();
			services.AddTransient<IInstaClient, InstaClient>();
			services.AddTransient<IHashtagLogic, HashtagLogic>();
			services.AddTransient<IHeartbeatLogic, HeartbeatLogic>();
			services.AddTransient<IResponseResolver, ResponseResolver>();
			services.AddTransient<ILookupLogic, LookupLogic>();
			services.AddTransient<ILookupCache, LookupCache>();
			services.AddSingleton<IGoogleSearchLogic, GoogleSearchLogic>();
			services.AddSingleton<IYandexImageSearch, YandexImageSearch>();
			services.AddSingleton<ITopicLookupLogic, TopicLookupLogic>();
			services.AddSingleton<ISearchProvider, SearchProvider>();
			services.AddSingleton<IHeartbeatService, HeartbeatService>();
			services.AddTransient<ITimelineEventLogLogic, TimelineEventLogLogic>();
		}
		internal static void IncludeConfigurators(this IServiceCollection services)
		{
			var accessors = new Config().Environments;
			services.Configure<GoogleSearchOptions>(options => { options.Endpoint = accessors.ImageSearchEndpoint; });
			services.Configure<RedisOptions>(o =>
			{
				o.ConnectionString = accessors.RedisConnectionString;
				o.DefaultKeyExpiry = TimeSpan.FromDays(7);
			});
			services.Configure<SeleniumLaunchOptions>(options =>
			{
				options.ChromePath = accessors.SeleniumChromeAddress;
			});

			BsonSerializer.RegisterSerializer(typeof(Guid), new GuidSerializer(BsonType.String));

			services.AddTransient<IMongoClientContext, MongoClientContext>(s =>
				new MongoClientContext(new MongoOptions
				{
					ConnectionString = accessors.ConnectionString,
					AccountCreatorDatabase = accessors.AccountCreationDatabase,
					AccountDatabase = accessors.MainDatabase,
					SchedulerDatabase = accessors.SchedulerDatabase,
					ControlDatabase = accessors.ControlDatabase,
					ContentDatabase = accessors.ContentDatabase
				}));
		}
		internal static void IncludeRepositories(this IServiceCollection services)
		{
			services.AddTransient<IInstagramAccountRepository, InstagramAccountRepository>();
			services.AddTransient<IProxyAssignmentsRepository, ProxyAssignmentsRepository>();
			services.AddTransient<IProfileRepository, ProfileRepository>();
			services.AddTransient<IHashtagsRepository, HashtagsRepository>();
			services.AddTransient<IHeartbeatRepository, HeartbeatRepository>();
			services.AddTransient<IInstagramAccountRedis, InstagramAccountRedis>();
			services.AddTransient<ILoggerStore, LoggerStore>();
			services.AddTransient<IAccountCache, AccountCache>();
			services.AddTransient<ISearchingCache, SearchingCache>();
			services.AddTransient<ITopicLookupRepository, TopicLookupRepository>();
			services.AddTransient<IRedisClient, RedisClient>();
			services.AddTransient<IReportHandlerRepository, ReportHandlerRepository>();
			services.AddTransient<ITimelineLoggingRepository, TimelineLoggingRepository>();
		}
		public static void IncludeHandlers(this IServiceCollection services)
		{
			services.AddTransient<IReportHandler, ReportHandler>();
			services.AddSingleton<IRestSharpClientManager, RestSharpClientManager>();
			services.AddTransient<ISeleniumClient, SeleniumClient>();
			services.AddTransient<IClientContextProvider, ClientContextProvider>();
			services.AddTransient<IApiClientContext, ApiClientContext>();
			services.AddTransient<IApiClientContainer, ApiClientContainer>();
			services.AddSingleton<IWorkerManager, WorkerManager>();
		}
		public static void IncludeContexts(this IServiceCollection services)
		{
			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
			services.AddTransient<IUserContext, UserContext>();
		}
		public static void IncludeEventServices(this IServiceCollection services)
		{
			services.AddTransient<IEventSubscriber<InstagramAccountPublishEventModel>, ProfileLogic>();
			services.AddTransient<IEventSubscriber<ProfileTopicAddRequest>, TopicLookupLogic>();
			services.AddTransient<IEventPublisher, EventPublisher>(
				s => new EventPublisher(services.BuildServiceProvider(false).CreateScope()));
		}
	}
}
