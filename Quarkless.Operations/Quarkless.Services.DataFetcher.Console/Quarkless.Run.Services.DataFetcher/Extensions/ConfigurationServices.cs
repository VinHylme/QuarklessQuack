using System;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Quarkless.Base.Auth.Common.Models.Interfaces;
using Quarkless.Base.AuthDetails.Logic;
using Quarkless.Base.ContentSearch.Logic;
using Quarkless.Base.ContentSearch.Models.Interfaces;
using Quarkless.Base.ContentSearch.Repository;
using Quarkless.Base.Hashtag.Logic;
using Quarkless.Base.Hashtag.Models.Interfaces;
using Quarkless.Base.Hashtag.Repository;
using Quarkless.Base.HashtagGenerator.Logic;
using Quarkless.Base.HashtagGenerator.Models.Interfaces;
using Quarkless.Base.Heartbeat.Logic;
using Quarkless.Base.Heartbeat.Models.Interfaces;
using Quarkless.Base.Heartbeat.Repository;
using Quarkless.Base.InstagramAccounts.Logic;
using Quarkless.Base.InstagramAccounts.Models.Interfaces;
using Quarkless.Base.InstagramAccounts.Repository.Mongo;
using Quarkless.Base.InstagramClient.Logic;
using Quarkless.Base.InstagramClient.Models.Interfaces;
using Quarkless.Base.InstagramComments.Logic;
using Quarkless.Base.InstagramComments.Models.Interfaces;
using Quarkless.Base.InstagramComments.Repository;
using Quarkless.Base.Lookup.Logic;
using Quarkless.Base.Lookup.Models.Interfaces;
using Quarkless.Base.Lookup.Repository;
using Quarkless.Base.Media.Logic;
using Quarkless.Base.Media.Models.Interfaces;
using Quarkless.Base.Media.Repository;
using Quarkless.Base.Notification.Logic;
using Quarkless.Base.Notification.Models.Interfaces;
using Quarkless.Base.Notification.Repository;
using Quarkless.Base.Profile.Logic;
using Quarkless.Base.Profile.Models.Interfaces;
using Quarkless.Base.Profile.Repository;
using Quarkless.Base.Proxy.Logic;
using Quarkless.Base.Proxy.Models;
using Quarkless.Base.Proxy.Models.Interfaces;
using Quarkless.Base.Proxy.Repository;
using Quarkless.Base.ReportHandler.Logic;
using Quarkless.Base.ReportHandler.Models.Interfaces;
using Quarkless.Base.ReportHandler.Repository;
using Quarkless.Base.ResponseResolver.Logic;
using Quarkless.Base.ResponseResolver.Models.Interfaces;
using Quarkless.Base.RestSharpClientManager.Logic;
using Quarkless.Base.RestSharpClientManager.Models.Interfaces;
using Quarkless.Base.TextGenerator.Logic;
using Quarkless.Base.TextGenerator.Models.Interfaces;
using Quarkless.Base.Topic.Logic;
using Quarkless.Base.Topic.Models.Interfaces;
using Quarkless.Base.Topic.Repository;
using Quarkless.Base.Utilities.Logic;
using Quarkless.Base.Utilities.Models.Interfaces;
using Quarkless.Base.WorkerManager.Logic;
using Quarkless.Base.WorkerManager.Models.Interfaces;
using Quarkless.Events;
using Quarkless.Events.Interfaces;
using Quarkless.Events.Models;
using Quarkless.Geolocation;
using Quarkless.Models.Shared.Extensions;
using Quarkless.Repository.MongoContext;
using Quarkless.Repository.MongoContext.Models;
using Quarkless.Repository.RedisContext;
using Quarkless.Repository.RedisContext.Models;
using Quarkless.Run.Services.DataFetcher.Models.Interfaces;

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
			//services.AddSingleton<ITranslateService, TranslateService>();
			services.AddSingleton<ISearchProvider, SearchProvider>();
			services.AddSingleton<ITopicLookupLogic, TopicLookupLogic>();
			services.AddTransient<IEventSubscriber<ProfileTopicAddRequest>, TopicLookupLogic>();
			services.AddSingleton<IProfileRepository, ProfileRepository>();
			services.AddSingleton<ITopicLookupRepository, TopicLookupRepository>();
			services.AddSingleton<IGoogleSearchLogic, GoogleSearchLogic>();
			services.AddSingleton<IRestSharpClientManager, RestSharpClientManager>();
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
			services.AddTransient<IProxyAssignmentsRepository, ProxyAssignmentsRepository>();
			services.AddTransient<IProfileLogic, ProfileLogic>();
			services.AddTransient<IEventSubscriber<InstagramAccountPublishEventModel>, ProfileLogic>();
			services.AddScoped<IResponseResolver, ResponseResolver>();
			services.AddSingleton<ILookupLogic, LookupLogic>();
			services.AddSingleton<ILookupCache, LookupCache>();
			services.AddSingleton<INotificationLogic, NotificationLogic>();
			services.AddSingleton<INotificationRepository, NotificationRepository>();
			services.AddSingleton<ISearchingCache, SearchingCache>();

			services.AddSingleton<IGeoLocationHandler, GeoLocationHandler>(s =>
				new GeoLocationHandler(new GeoLocationOptions
				{
					IpGeolocationToken = accessors.IpGeoLocationApiKey,
					GeonamesToken = accessors.GeonamesApiKey,
					GoogleGeocodeToken = accessors.GoogleGeocodeApiKey
				}));

			services.AddTransient<IProxyRequest, ProxyRequest>(s => new ProxyRequest(
				new ProxyRequestOptions(accessors.ProxyHandlerApiEndPoint),
				s.GetService<IGeoLocationHandler>(),
				s.GetService<IProxyAssignmentsRepository>()));

			services.AddTransient<IEventPublisher, EventPublisher>(s => new EventPublisher(services));

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
					StatisticsDatabase = accessors.StatisticsDatabase,
					ControlDatabase = accessors.ControlDatabase,
					ContentDatabase = accessors.ContentDatabase
				}));
		}
	}
}
