using System;
using System.Runtime.InteropServices;
using Amazon;
using Amazon.CognitoIdentity;
using Amazon.CognitoIdentityProvider;
using Amazon.Extensions.CognitoAuthentication;
using Amazon.Extensions.NETCore.Setup;
using Amazon.S3;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDbGenericRepository;
using Newtonsoft.Json;
using Quarkless.Analyser;
using Quarkless.Analyser.Models;
using Quarkless.Base.ContentSearch;
using Quarkless.Base.InstagramComments;
using Quarkless.Events;
using Quarkless.Events.Interfaces;
using Quarkless.Logic.Agent;
using Quarkless.Logic.Auth;
using Quarkless.Logic.Auth.Manager;
using Quarkless.Logic.Comments;
using Quarkless.Logic.ContentInfo;
using Quarkless.Logic.ContentSearch;
using Quarkless.Logic.Hashtag;
using Quarkless.Logic.HashtagGenerator;
using Quarkless.Logic.Heartbeat;
using Quarkless.Logic.InstagramAccounts;
using Quarkless.Logic.InstagramClient;
using Quarkless.Logic.Library;
using Quarkless.Logic.Lookup;
using Quarkless.Logic.Media;
using Quarkless.Logic.Messaging;
using Quarkless.Logic.Profile;
using Quarkless.Logic.Proxy;
using Quarkless.Logic.ReportHandler;
using Quarkless.Logic.RequestBuilder;
using Quarkless.Logic.ResponseResolver;
using Quarkless.Logic.RestSharpClientManager;
using Quarkless.Logic.SeleniumClient;
using Quarkless.Logic.Services.Automation;
using Quarkless.Logic.Storage;
using Quarkless.Logic.TextGenerator;
using Quarkless.Logic.Timeline;
using Quarkless.Logic.Timeline.TaskScheduler;
using Quarkless.Logic.Topic;
using Quarkless.Logic.Utilities;
using Quarkless.Models.Agent.Interfaces;
using Quarkless.Models.ApiLogger.Interfaces;
using Quarkless.Models.Auth;
using Quarkless.Models.Auth.AccountContext;
using Quarkless.Models.Auth.Enums;
using Quarkless.Models.Auth.Interfaces;
using Quarkless.Models.Comments.Interfaces;
using Quarkless.Models.ContentInfo.Interfaces;
using Quarkless.Models.ContentSearch.Interfaces;
using Quarkless.Models.ContentSearch.Models;
using Quarkless.Models.Hashtag.Interfaces;
using Quarkless.Models.HashtagGenerator.Interfaces;
using Quarkless.Models.Heartbeat.Interfaces;
using Quarkless.Models.InstagramAccounts.Interfaces;
using Quarkless.Models.InstagramClient.Interfaces;
using Quarkless.Models.Library.Interfaces;
using Quarkless.Models.Lookup.Interfaces;
using Quarkless.Models.Media.Interfaces;
using Quarkless.Models.Messaging.Interfaces;
using Quarkless.Models.Profile.Interfaces;
using Quarkless.Models.Proxy.Interfaces;
using Quarkless.Models.ReportHandler.Interfaces;
using Quarkless.Models.RequestBuilder.Interfaces;
using Quarkless.Models.ResponseResolver.Interfaces;
using Quarkless.Models.RestSharpClientManager.Interfaces;
using Quarkless.Models.SeleniumClient;
using Quarkless.Models.SeleniumClient.Interfaces;
using Quarkless.Models.Services.Automation.Interfaces;
using Quarkless.Models.Shared.Api.Extensions;
using Quarkless.Models.Shared.Extensions;
using Quarkless.Models.Storage;
using Quarkless.Models.Storage.Interfaces;
using Quarkless.Models.TextGenerator.Interfaces;
using Quarkless.Models.Timeline.Interfaces;
using Quarkless.Models.Timeline.Interfaces.TaskScheduler;
using Quarkless.Models.Topic.Interfaces;
using Quarkless.Models.TranslateService;
using Quarkless.Models.Utilities.Interfaces;
using Quarkless.Repository.ApiLogger;
using Quarkless.Repository.Auth;
using Quarkless.Repository.Comments;
using Quarkless.Repository.ContentSearch;
using Quarkless.Repository.Hashtag;
using Quarkless.Repository.Heartbeat;
using Quarkless.Repository.InstagramAccounts.Mongo;
using Quarkless.Repository.InstagramAccounts.Redis;
using Quarkless.Repository.Library;
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
using Quarkless.Vision;

namespace Quarkless.Run.Services.Automation.Extensions
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
			//services.AddSingleton<IDiscoverLogic, DiscoverLogic>();
			services.AddTransient<IProxyLogic, ProxyLogic>();
			services.AddTransient<IInstagramAccountLogic, InstagramAccountLogic>();
			services.AddTransient<IProfileLogic, ProfileLogic>();
			//services.AddTransient<IInstaUserLogic, InstaUserLogic>();
			services.AddTransient<ICommentLogic, CommentLogic>();
			//services.AddTransient<ICollectionsLogic, CollectionsLogic>();
			//services.AddTransient<IInstaAccountOptionsLogic, InstaAccountOptionsLogic>();
			services.AddTransient<IInstaClient, InstaClient>();
			services.AddTransient<IHashtagLogic, HashtagLogic>();
			services.AddTransient<IMediaLogic, MediaLogic>();
			services.AddTransient<ITimelineLogic, TimelineLogic>();
			services.AddTransient<IHeartbeatLogic, HeartbeatLogic>();
			services.AddTransient<IAgentLogic, AgentLogic>();
			services.AddTransient<ICommentCorpusLogic, CommentCorpusLogic>();
			services.AddTransient<IMediaCorpusLogic, MediaCorpusLogic>();
			//services.AddTransient<IQueryLogic, QueryLogic>();
			//services.AddSingleton<IInstagramContentSearch, InstagramContentSearch>();
			services.AddTransient<ILibraryLogic, LibraryLogic>();
			services.AddTransient<ITimelineEventLogLogic, TimelineEventLogLogic>();
			services.AddTransient<IResponseResolver, ResponseResolver>();
			//services.AddTransient<IBusinessLogic, BusinessLogic>();
			services.AddTransient<IMessagingLogic, MessagingLogic>();
			services.AddTransient<ILookupLogic, LookupLogic>();
			services.AddTransient<ILookupCache, LookupCache>();
			//services.AddSingleton<IWebHookHandler, StripeWebHookHandler>();
			services.AddSingleton<IGoogleSearchLogic, GoogleSearchLogic>();
			services.AddSingleton<IYandexImageSearch, YandexImageSearch>();
			services.AddSingleton<IPostAnalyser, PostAnalyser>();
			services.AddSingleton<IMediaManipulation, MediaManipulation>();
			services.AddSingleton<ITopicLookupLogic, TopicLookupLogic>();
			services.AddSingleton<ISearchProvider, SearchProvider>();
			services.AddSingleton<IVideoEditor, VideoEditor>();
			services.AddSingleton<IAudioEditor, AudioEditor>();
			services.AddSingleton<IAgentManager, AgentManager>();
		}
		internal static void IncludeConfigurators(this IServiceCollection services)
		{
			var accessors = new Config().Environments;

			services.Configure<S3Options>(options => { options.BucketName = accessors.S3BucketName; });
			services.Configure<GoogleSearchOptions>(options => { options.Endpoint = accessors.ImageSearchEndpoint; });
			services.Configure<RedisOptions>(o =>
			{
				o.ConnectionString = accessors.RedisConnectionString;
				o.DefaultKeyExpiry = TimeSpan.FromDays(7);
			});
			services.Configure<TranslateOptions>(options =>
			{
				options.DetectLangAPIKey = accessors.DetectApi;
				options.YandexAPIKey = accessors.YandexApiKey;
				options.NaturalLanguageAPIPath = accessors.NaturalLanguageApiPath;
			});
			services.Configure<SeleniumLaunchOptions>(options =>
			{
				options.ChromePath = accessors.SeleniumChromeAddress;
			});

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
			services.AddTransient<ITimelineLoggingRepository, TimelineLoggingRepository>();
			services.AddTransient<IHeartbeatRepository, HeartbeatRepository>();
			services.AddTransient<IInstagramAccountRedis, InstagramAccountRedis>();
			services.AddTransient<ICommentCorpusRepository, CommentCorpusRepository>();
			services.AddTransient<IMediaCorpusRepository, MediaCorpusRepository>();
			services.AddTransient<ITimelineJobRepository, TimelineJobRepository>();
			services.AddTransient<ILibraryRepository, LibraryRepository>();
			services.AddTransient<ILibraryCache, LibraryCache>();
			services.AddTransient<IApiLogCache, ApiLogCache>();
			services.AddTransient<ILoggerStore, LoggerStore>();
			services.AddTransient<IAccountCache, AccountCache>();
			services.AddTransient<ISearchingCache, SearchingCache>();
			services.AddTransient<ITopicLookupRepository, TopicLookupRepository>();
			services.AddTransient<IReportHandlerRepository, ReportHandlerRepository>();
			services.AddTransient<IRedisClient, RedisClient>();

			//services.AddTransient<IInstagramAccountCreatorRepository, InstagramAccountCreatorRepository>();
			//services.AddTransient<IEmailAccountCreatorRepository, EmailAccountCreatorRepository>();
		}
		public static void IncludeHandlers(this IServiceCollection services)
		{
			services.AddTransient<IReportHandler, ReportHandler>();
			services.AddSingleton<IRestSharpClientManager, RestSharpClientManager>();
			services.AddTransient<ISeleniumClient, SeleniumClient>();
			services.AddTransient<IClientContextProvider, ClientContextProvider>();
			services.AddTransient<IApiClientContext, ApiClientContext>();
			services.AddTransient<IApiClientContainer, ApiClientContainer>();
			//services.AddTransient<ITranslateService, TranslateService>();
			services.AddTransient<IUtilProviders, UtilProviders>();
			services.AddSingleton<ITextGenerator, TextGenerator>();
			services.AddSingleton<IHashtagGenerator, HashtagGenerator>();
			services.AddSingleton<IContentInfoBuilder, ContentInfoBuilder>();
			//services.AddSingleton<IWorkerManager, WorkerManager>();
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
				s => new EventPublisher(services.BuildServiceProvider(false).CreateScope()));
		}
		public static void IncludeAuthHandlers(this IServiceCollection services)
		{
			var accessors = new Config().Environments;
			var apiAccessors = new Configuration().Environments;

			Environment.GetEnvironmentVariable("JWT_KEY");
			Environment.SetEnvironmentVariable("AWS_ACCESS_KEY_ID", apiAccessors.AwsAccess.AccessKey);
			Environment.SetEnvironmentVariable("AWS_SECRET_ACCESS_KEY", apiAccessors.AwsAccess.SecretKey);
			Environment.SetEnvironmentVariable("AWS_REGION", apiAccessors.AwsAccess.Region);

			var regionEndpoint = RegionEndpoint.EUWest2;
			IAmazonCognitoIdentityProvider amazonCognitoIdentityProvider = new AmazonCognitoIdentityProviderClient(apiAccessors.AwsAccess.AccessKey, apiAccessors.AwsAccess.SecretKey, regionEndpoint);
			var userPool = new CognitoUserPool(apiAccessors.AwsPool.PoolID, apiAccessors.AwsPool.AppClientID, amazonCognitoIdentityProvider, apiAccessors.AwsPool.AppClientSecret);
			services.AddSingleton(amazonCognitoIdentityProvider);
			services.AddSingleton(userPool);

			services.AddAWSService<IAmazonS3>();

			services.AddSingleton<IAmazonS3>(new AmazonS3Client(apiAccessors.AwsAccess.AccessKey,
				apiAccessors.AwsAccess.SecretKey, new AmazonS3Config
				{
					RegionEndpoint = regionEndpoint,
					SignatureVersion = "v4",
					SignatureMethod = Amazon.Runtime.SigningAlgorithm.HmacSHA256
				}));

			services.AddSingleton<IS3BucketLogic, S3BucketLogic>();
			var mongoDbContext = new MongoDbContext(accessors.ConnectionString, "Accounts");

			services.AddIdentity<AccountUser, AccountRole>()
				.AddMongoDbStores<AccountUser, AccountRole, string>(mongoDbContext)
				.AddDefaultTokenProviders();

			services.AddDefaultAWSOptions(new AWSOptions
			{
				Region = regionEndpoint,
				Credentials = new CognitoAWSCredentials(userPool.PoolID, regionEndpoint),
				ProfilesLocation = apiAccessors.AwsOptions.ProfileLocation,
				Profile = apiAccessors.AwsOptions.Profile
			});

			services.AddAuthorization(
			auth => {
				auth.DefaultPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme‌​)
					.RequireAuthenticatedUser()
					.Build();
				auth.AddPolicy("TrialUsers", p => p.Requirements.Add(new GroupAuthorisationRequirement(AuthTypes.TrialUsers.ToString())));
				auth.AddPolicy("BasicUsers", p => p.Requirements.Add(new GroupAuthorisationRequirement(AuthTypes.BasicUsers.ToString())));
				auth.AddPolicy("PremiumUsers", p => p.Requirements.Add(new GroupAuthorisationRequirement(AuthTypes.PremiumUsers.ToString())));
				auth.AddPolicy("EnterpriseUsers", p => p.Requirements.Add(new GroupAuthorisationRequirement(AuthTypes.EnterpriseUsers.ToString())));
			});
			services.AddAuthentication(
				options =>
				{
					options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
					options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
				}).AddJwtBearer(
				o =>
				{

					o.Audience = apiAccessors.AwsPool.AppClientID;
					o.Authority = apiAccessors.AwsPool.AuthUrl;
					o.RequireHttpsMetadata = false;
					o.SaveToken = true;

				}
			);

			services.AddSingleton<IAuthAccessHandler>(new AuthAccessHandler(apiAccessors.AwsPool.AppClientSecret));
			services.AddScoped<IAuthHandler, AuthHandler>();
			services.AddScoped<IAuthorizationHandler, AuthClientHandler>();
		}
	}
}
