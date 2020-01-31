using System;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Quarkless.Base.ContentSearch;
using Quarkless.Events;
using Quarkless.Events.Interfaces;
using Quarkless.Logic.Comments;
using Quarkless.Logic.ContentSearch;
using Quarkless.Logic.Hashtag;
using Quarkless.Logic.HashtagGenerator;
using Quarkless.Logic.Heartbeat;
using Quarkless.Logic.InstagramAccounts;
using Quarkless.Logic.InstagramClient;
using Quarkless.Logic.Lookup;
using Quarkless.Logic.Media;
using Quarkless.Logic.Profile;
using Quarkless.Logic.Proxy;
using Quarkless.Logic.ReportHandler;
using Quarkless.Logic.ResponseResolver;
using Quarkless.Logic.RestSharpClientManager;
using Quarkless.Logic.SeleniumClient;
using Quarkless.Logic.TextGenerator;
using Quarkless.Logic.Timeline;
using Quarkless.Logic.Topic;
using Quarkless.Logic.TranslateService;
using Quarkless.Logic.Utilities;
using Quarkless.Logic.WorkerManager;
using Quarkless.Models.Auth;
using Quarkless.Models.Auth.Interfaces;
using Quarkless.Models.Comments.Interfaces;
using Quarkless.Models.ContentSearch.Interfaces;
using Quarkless.Models.ContentSearch.Models;
using Quarkless.Models.EmailService.Interfaces;
using Quarkless.Models.Hashtag.Interfaces;
using Quarkless.Models.HashtagGenerator.Interfaces;
using Quarkless.Models.Heartbeat.Interfaces;
using Quarkless.Models.InstagramAccounts;
using Quarkless.Models.InstagramAccounts.Interfaces;
using Quarkless.Models.InstagramClient.Interfaces;
using Quarkless.Models.Lookup.Interfaces;
using Quarkless.Models.Media.Interfaces;
using Quarkless.Models.Profile;
using Quarkless.Models.Profile.Interfaces;
using Quarkless.Models.Proxy;
using Quarkless.Models.Proxy.Interfaces;
using Quarkless.Models.ReportHandler.Interfaces;
using Quarkless.Models.ResponseResolver.Interfaces;
using Quarkless.Models.RestSharpClientManager.Interfaces;
using Quarkless.Models.SeleniumClient.Interfaces;
using Quarkless.Models.Services.DataFetcher.Interfaces;
using Quarkless.Models.Shared.Extensions;
using Quarkless.Models.TextGenerator.Interfaces;
using Quarkless.Models.Timeline.Interfaces;
using Quarkless.Models.Topic.Interfaces;
using Quarkless.Models.TranslateService.Interfaces;
using Quarkless.Models.Utilities.Interfaces;
using Quarkless.Models.WorkerManager.Interfaces;
using Quarkless.Repository.Comments;
using Quarkless.Repository.ContentSearch;
using Quarkless.Repository.Hashtag;
using Quarkless.Repository.Heartbeat;
using Quarkless.Repository.InstagramAccounts.Mongo;
using Quarkless.Repository.Lookup;
using Quarkless.Repository.Media;
using Quarkless.Repository.MongoContext;
using Quarkless.Repository.MongoContext.Models;
using Quarkless.Repository.Profile;
using Quarkless.Repository.Proxy;
using Quarkless.Repository.RedisContext;
using Quarkless.Repository.RedisContext.Models;
using Quarkless.Repository.ReportHandler;
using Quarkless.Repository.Timeline;
using Quarkless.Repository.Topic;

namespace Quarkless.Run.Services.DataFetcher.Extensions
{
	internal static class ConfigurationServices
	{
		internal static void IncludeServices(this IServiceCollection services)
		{
			var accessors = new Config().Environments;
			services.AddTransient<IFetchResolver, FetchResolver>();
			services.AddSingleton<IMediaCorpusLogic, MediaCorpusLogic>();
			services.AddSingleton<IMediaCorpusRepository, MediaCorpusRepository>();
			services.AddSingleton<ICommentCorpusLogic, CommentCorpusLogic>();
			services.AddSingleton<ICommentCorpusRepository, CommentCorpusRepository>();
			services.AddSingleton<IHashtagLogic, HashtagLogic>();
			services.AddSingleton<IHashtagsRepository, HashtagsRepository>();
			services.AddSingleton<IReportHandler, ReportHandler>();
			services.AddSingleton<ILoggerStore, LoggerStore>();
			services.AddSingleton<IReportHandlerRepository, ReportHandlerRepository>();
			services.AddSingleton<IHeartbeatLogic, HeartbeatLogic>();
			services.AddSingleton<IHeartbeatRepository, HeartbeatRepository>();
			services.AddSingleton<IUtilProviders, UtilProviders>();
			services.AddSingleton<ITextGenerator, TextGenerator>();
			services.AddSingleton<IHashtagGenerator, HashtagGenerator>();
			services.AddSingleton<ITranslateService, TranslateService>();
			services.AddSingleton<ISearchProvider, SearchProvider>();
			services.AddSingleton<ITopicLookupLogic, TopicLookupLogic>();
			services.AddTransient<IEventSubscriber<ProfileTopicAddRequest>, TopicLookupLogic>();
			services.AddSingleton<IProfileRepository, ProfileRepository>();
			services.AddSingleton<ITopicLookupRepository, TopicLookupRepository>();
			services.AddSingleton<IGoogleSearchLogic, GoogleSearchLogic>();
			services.AddSingleton<IRestSharpClientManager, RestSharpClientManager>();
			services.AddSingleton<ISeleniumClient, SeleniumClient>();
			services.AddSingleton<IWorkerManager, WorkerManager>();
			services.AddTransient<IApiClientContext, ApiClientContext>();
			services.AddTransient<IApiClientContainer, ApiClientContainer>();
			services.AddTransient<IUserContext, UserContext>();
			services.AddHttpContextAccessor();
			services.AddTransient<IClientContextProvider, ClientContextProvider>();
			services.AddTransient<IInstagramAccountLogic, InstagramAccountLogic>();
			services.AddTransient<IInstagramAccountRepository, InstagramAccountRepository>();
			services.AddTransient<IRedisClient, RedisClient>();
			services.AddTransient<IProxyLogic, ProxyLogic>();
			services.AddTransient<IProxyRequest, ProxyRequest>(s =>
				new ProxyRequest(new ProxyRequestOptions(accessors.ProxyHandlerApiEndPoint)));
			services.AddTransient<IProxyAssignmentsRepository, ProxyAssignmentsRepository>();
			services.AddTransient<IProfileLogic, ProfileLogic>();
			services.AddTransient<IEventSubscriber<InstagramAccountPublishEventModel>, ProfileLogic>();
			services.AddScoped<IInstaClient, InstaClient>();
			services.AddScoped<IResponseResolver, ResponseResolver>();
			services.AddSingleton<ILookupLogic, LookupLogic>();
			services.AddSingleton<ILookupCache, LookupCache>();
			services.AddSingleton<ITimelineEventLogLogic, TimelineEventLogLogic>();
			services.AddSingleton<ITimelineLoggingRepository, TimelineLoggingRepository>();
			services.AddSingleton<ISearchingCache, SearchingCache>();

			services.Configure<GoogleSearchOptions>(options => { options.Endpoint = accessors.ImageSearchEndpoint; });
			services.AddTransient<IEventPublisher, EventPublisher>(
				s => new EventPublisher(services.BuildServiceProvider(false).CreateScope()));
			services.Configure<RedisOptions>(o =>
			{
				o.ConnectionString = accessors.RedisConnectionString;
				o.DefaultKeyExpiry = TimeSpan.FromDays(7);
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
	}
}
