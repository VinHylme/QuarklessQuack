using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Quarkless.Base.AccountOptions;
using Quarkless.Base.Auth.Common.Models.Interfaces;
using Quarkless.Base.AuthDetails.Logic;
using Quarkless.Base.AuthDetails.Repository;
using Quarkless.Base.ContentSearch.Logic;
using Quarkless.Base.ContentSearch.Models.Interfaces;
using Quarkless.Base.ContentSearch.Repository;
using Quarkless.Base.Hashtag.Logic;
using Quarkless.Base.Hashtag.Models.Interfaces;
using Quarkless.Base.Hashtag.Repository;
using Quarkless.Base.Heartbeat.Logic;
using Quarkless.Base.Heartbeat.Models.Interfaces;
using Quarkless.Base.Heartbeat.Repository;
using Quarkless.Base.InstagramAccounts.Logic;
using Quarkless.Base.InstagramAccounts.Models.Interfaces;
using Quarkless.Base.InstagramAccounts.Repository.Mongo;
using Quarkless.Base.InstagramAccounts.Repository.Redis;
using Quarkless.Base.InstagramClient.Logic;
using Quarkless.Base.InstagramClient.Models.Interfaces;
using Quarkless.Base.InstagramComments.Logic;
using Quarkless.Base.InstagramComments.Models.Interfaces;
using Quarkless.Base.Lookup.Logic;
using Quarkless.Base.Lookup.Models.Interfaces;
using Quarkless.Base.Lookup.Repository;
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
using Quarkless.Base.PuppeteerClient.Logic;
using Quarkless.Base.PuppeteerClient.Models.Interfaces;
using Quarkless.Base.ReportHandler.Logic;
using Quarkless.Base.ReportHandler.Models.Interfaces;
using Quarkless.Base.ReportHandler.Repository;
using Quarkless.Base.ResponseResolver.Logic;
using Quarkless.Base.ResponseResolver.Models.Interfaces;
using Quarkless.Base.RestSharpClientManager.Logic;
using Quarkless.Base.RestSharpClientManager.Models.Interfaces;
using Quarkless.Base.Topic.Logic;
using Quarkless.Base.Topic.Models.Interfaces;
using Quarkless.Base.Topic.Repository;
using Quarkless.Events;
using Quarkless.Events.Interfaces;
using Quarkless.Events.Models;
using Quarkless.Geolocation;
using Quarkless.Models.Shared.Extensions;
using Quarkless.Repository.MongoContext;
using Quarkless.Repository.MongoContext.Models;
using Quarkless.Repository.RedisContext;
using Quarkless.Repository.RedisContext.Models;
using HeartbeatService = Quarkless.Run.Services.Heartbeat.Logic.HeartbeatService;
using IHeartbeatService = Quarkless.Run.Services.Heartbeat.Models.Interfaces.IHeartbeatService;

namespace Quarkless.Run.Services.Heartbeat.Models.Extensions
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
			services.AddTransient<INotificationLogic, NotificationLogic>();
		}
		internal static void IncludeConfigurators(this IServiceCollection services)
		{
			var accessors = new Config().Environments;
			services.Configure<RedisOptions>(o =>
			{
				o.ConnectionString = accessors.RedisConnectionString;
				o.DefaultKeyExpiry = TimeSpan.FromDays(7);
			});
			
			BsonSerializer.RegisterSerializer(typeof(Guid), new GuidSerializer(BsonType.String));

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

			services.AddSingleton<IPuppeteerClient, PuppeteerClient>(s=>new PuppeteerClient(2));

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
			services.AddTransient<INotificationRepository, NotificationRepository>();
		}
		public static void IncludeHandlers(this IServiceCollection services)
		{
			services.AddTransient<IReportHandler, ReportHandler>();
			services.AddSingleton<IRestSharpClientManager, RestSharpClientManager>();
			services.AddTransient<IClientContextProvider, ClientContextProvider>();
			services.AddTransient<IApiClientContext, ApiClientContext>();
			services.AddTransient<IApiClientContainer, ApiClientContainer>();
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
				s => new EventPublisher(services));
		}
	}
}
