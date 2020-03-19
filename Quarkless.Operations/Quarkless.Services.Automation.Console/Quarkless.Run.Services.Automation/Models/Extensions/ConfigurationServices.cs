using System;
using System.Runtime.InteropServices;
using Amazon;
using Amazon.S3;
using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Quarkless.Analyser;
using Quarkless.Analyser.Models;
using Quarkless.Base.Actions.Logic.Factory.ActionBuilder.Manager;
using Quarkless.Base.Actions.Models.Interfaces;
using Quarkless.Base.Agent.Logic;
using Quarkless.Base.Agent.Models.Interfaces;
using Quarkless.Base.ApiLogger.Models.Interfaces;
using Quarkless.Base.ApiLogger.Repository;
using Quarkless.Base.Auth.Common.Models.Interfaces;
using Quarkless.Base.AuthDetails.Logic;
using Quarkless.Base.AuthDetails.Repository;
using Quarkless.Base.ContentInfo.Logic;
using Quarkless.Base.ContentInfo.Models.Interfaces;
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
using Quarkless.Base.InstagramAccounts.Repository.Redis;
using Quarkless.Base.InstagramClient.Logic;
using Quarkless.Base.InstagramClient.Models.Interfaces;
using Quarkless.Base.InstagramComments.Logic;
using Quarkless.Base.InstagramComments.Models.Interfaces;
using Quarkless.Base.InstagramComments.Repository;
using Quarkless.Base.Library.Logic;
using Quarkless.Base.Library.Models.Interfaces;
using Quarkless.Base.Library.Repository;
using Quarkless.Base.Lookup.Logic;
using Quarkless.Base.Lookup.Models.Interfaces;
using Quarkless.Base.Lookup.Repository;
using Quarkless.Base.Media.Logic;
using Quarkless.Base.Media.Models.Interfaces;
using Quarkless.Base.Media.Repository;
using Quarkless.Base.Messaging.Logic;
using Quarkless.Base.Messaging.Models.Interfaces;
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
using Quarkless.Base.RequestBuilder.Logic;
using Quarkless.Base.RequestBuilder.Models.Interfaces;
using Quarkless.Base.ResponseResolver.Logic;
using Quarkless.Base.ResponseResolver.Models.Interfaces;
using Quarkless.Base.RestSharpClientManager.Logic;
using Quarkless.Base.RestSharpClientManager.Models.Interfaces;
using Quarkless.Base.Storage.Logic;
using Quarkless.Base.Storage.Models;
using Quarkless.Base.Storage.Models.Interfaces;
using Quarkless.Base.TextGenerator.Logic;
using Quarkless.Base.TextGenerator.Models.Interfaces;
using Quarkless.Base.Timeline.Logic;
using Quarkless.Base.Timeline.Logic.TaskScheduler;
using Quarkless.Base.Timeline.Models.Interfaces;
using Quarkless.Base.Timeline.Models.Interfaces.TaskScheduler;
using Quarkless.Base.Topic.Logic;
using Quarkless.Base.Topic.Models.Interfaces;
using Quarkless.Base.Topic.Repository;
using Quarkless.Base.Utilities.Logic;
using Quarkless.Base.Utilities.Models.Interfaces;
using Quarkless.Events;
using Quarkless.Events.Interfaces;
using Quarkless.Geolocation;
using Quarkless.Models.Shared.Api.Extensions;
using Quarkless.Models.Shared.Extensions;
using Quarkless.Repository.MongoContext;
using Quarkless.Repository.MongoContext.Models;
using Quarkless.Repository.RedisContext;
using Quarkless.Repository.RedisContext.Models;
using Quarkless.Vision;
using AgentManager = Quarkless.Run.Services.Automation.Logic.AgentManager;
using IAgentManager = Quarkless.Run.Services.Automation.Models.Interfaces.IAgentManager;

namespace Quarkless.Run.Services.Automation.Models.Extensions
{
	public static class ConfigurationServices
	{
		private static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
		internal static void IncludeHangFrameworkServices(this IServiceCollection serviceCollection)
		{
			serviceCollection.AddTransient<ITaskService, TaskService>();
			serviceCollection.AddTransient<IBackgroundJobClient, BackgroundJobClient>();
			serviceCollection.AddTransient<IJobRunner, JobRunner>();
		}
		internal static void IncludeLogicServices(this IServiceCollection services)
		{
			
			services.AddTransient<IProxyLogic, ProxyLogic>();
			services.AddTransient<IInstagramAccountLogic, InstagramAccountLogic>();
			services.AddTransient<IProfileLogic, ProfileLogic>();
			services.AddTransient<ICommentLogic, CommentLogic>();
			services.AddTransient<IHashtagLogic, HashtagLogic>();
			services.AddTransient<IMediaLogic, MediaLogic>();
			services.AddTransient<ITimelineLogic, TimelineLogic>();
			services.AddTransient<IHeartbeatLogic, HeartbeatLogic>();
			services.AddTransient<IAgentLogic, AgentLogic>();
			services.AddTransient<ICommentCorpusLogic, CommentCorpusLogic>();
			services.AddTransient<IMediaCorpusLogic, MediaCorpusLogic>();
			services.AddTransient<ILibraryLogic, LibraryLogic>();
			services.AddTransient<INotificationLogic, NotificationLogic>();
			services.AddTransient<IResponseResolver, ResponseResolver>();
			services.AddTransient<IMessagingLogic, MessagingLogic>();
			services.AddTransient<ILookupLogic, LookupLogic>();
			services.AddTransient<ILookupCache, LookupCache>();
			services.AddSingleton<IGoogleSearchLogic, GoogleSearchLogic>();
			services.AddSingleton<IYandexImageSearch, YandexImageSearch>();
			services.AddSingleton<IPostAnalyser, PostAnalyser>();
			services.AddSingleton<IMediaManipulation, MediaManipulation>();
			services.AddSingleton<ITopicLookupLogic, TopicLookupLogic>();
			services.AddSingleton<ISearchProvider, SearchProvider>();
			services.AddSingleton<IVideoEditor, VideoEditor>();
			services.AddSingleton<IAudioEditor, AudioEditor>();
			services.AddScoped<IAgentManager, AgentManager>();
			services.AddScoped<IActionCommitFactory, ActionFactoryManager>();
		}
		internal static void IncludeConfigurators(this IServiceCollection services)
		{
			var accessors = new Config().Environments;
			var apiAccessors = new Configuration().Environments;

			services.Configure<S3Options>(options => { options.BucketName = accessors.S3BucketName; });
			services.AddAWSService<IAmazonS3>();
			var regionEndpoint = RegionEndpoint.EUWest2;

			services.AddSingleton<IAmazonS3>(new AmazonS3Client(apiAccessors.AwsAccess.AccessKey,
				apiAccessors.AwsAccess.SecretKey, new AmazonS3Config
				{
					RegionEndpoint = regionEndpoint,
					SignatureVersion = "v4",
					SignatureMethod = Amazon.Runtime.SigningAlgorithm.HmacSHA256
				}));

			services.AddSingleton<IS3BucketLogic, S3BucketLogic>();
			services.Configure<RedisOptions>(o =>
			{
				o.ConnectionString = accessors.RedisConnectionString;
				o.DefaultKeyExpiry = TimeSpan.FromDays(7);
			});
			
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

			// var stripeCredentials = JsonConvert.DeserializeObject<StripeCredentials>(accessors.JsonStripeCredentials);
			//
			// services.AddSingleton<IAccountLogic, AccountLogic>(s =>
			// 	new AccountLogic(new AccountOptions
			// 	{
			// 		StripeKey = stripeCredentials
			// 	}));

			services.AddSingleton<IFfmpegWrapper>(new FfmpegWrapper(new MediaAnalyserOptions
			{
				TempAudioPath = accessors.TempAudioPath,
				TempImagePath = accessors.TempImagePath,
				TempVideoPath = accessors.TempVideoPath,
				FfmpegEnginePath = accessors.FfmpegPath,
				IsOnWindows = IsWindows
			}));

			services.AddSingleton<IUrlReader>(new UrlReader(accessors.ApiBasePath));

			services.AddSingleton<IVisionClient, VisionClient>
				(s => new VisionClient(accessors.VisionCredentials, s.GetService<ISearchingCache>()));

			services.AddSingleton<IPuppeteerClient, PuppeteerClient>(s => new PuppeteerClient(2));
			
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
		internal static void IncludeRepositories(this IServiceCollection services)
		{
			services.AddTransient<IInstagramAccountRepository, InstagramAccountRepository>();
			services.AddTransient<IProxyAssignmentsRepository, ProxyAssignmentsRepository>();
			services.AddTransient<IProfileRepository, ProfileRepository>();
			services.AddTransient<IHashtagsRepository, HashtagsRepository>();
			services.AddTransient<INotificationRepository, NotificationRepository>();
			services.AddTransient<IHeartbeatRepository, HeartbeatRepository>();
			services.AddTransient<IInstagramAccountRedis, InstagramAccountRedis>();
			services.AddTransient<ICommentCorpusRepository, CommentCorpusRepository>();
			services.AddTransient<IMediaCorpusRepository, MediaCorpusRepository>();
			services.AddTransient<ILibraryRepository, LibraryRepository>();
			services.AddTransient<ILibraryCache, LibraryCache>();
			services.AddTransient<IApiLogCache, ApiLogCache>();
			services.AddTransient<ILoggerStore, LoggerStore>();
			services.AddTransient<IAccountCache, AccountCache>();
			services.AddTransient<ISearchingCache, SearchingCache>();
			services.AddTransient<ITopicLookupRepository, TopicLookupRepository>();
			services.AddTransient<IReportHandlerRepository, ReportHandlerRepository>();
			services.AddTransient<IRedisClient, RedisClient>();
			services.AddTransient<IAccountRepository, AccountRepository>();
			//services.AddTransient<IInstagramAccountCreatorRepository, InstagramAccountCreatorRepository>();
			//services.AddTransient<IEmailAccountCreatorRepository, EmailAccountCreatorRepository>();
		}
		public static void IncludeHandlers(this IServiceCollection services)
		{
			services.AddTransient<IReportHandler, ReportHandler>();
			services.AddSingleton<IRestSharpClientManager, RestSharpClientManager>();
			services.AddTransient<IClientContextProvider, ClientContextProvider>();
			services.AddTransient<IApiClientContext, ApiClientContext>();
			services.AddTransient<IApiClientContainer, ApiClientContainer>();
			//services.AddTransient<ITranslateService, TranslateService>();
			services.AddTransient<IUtilProviders, UtilProviders>();
			services.AddSingleton<ITextGenerator, TextGenerator>();
			services.AddSingleton<IHashtagGenerator, HashtagGenerator>();
			services.AddSingleton<IContentInfoBuilder, ContentInfoBuilder>();
		}
		public static void IncludeContexts(this IServiceCollection services)
		{
			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
			services.AddTransient<IUserContext, UserContext>();
			services.AddTransient<IRequestBuilder, RequestBuilder>();
		}
		public static void IncludeEventServices(this IServiceCollection services)
		{
			//services.AddTransient<IEventSubscriber<InstagramAccountPublishEventModel>, ProfileLogic>();
			//services.AddTransient<IEventSubscriber<ProfileTopicAddRequest>, TopicLookupLogic>();
			services.AddTransient<IEventPublisher, EventPublisher>(
				s => new EventPublisher(services));
		}
	}
}
