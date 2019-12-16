#region References
using System;
using System.Linq;
using System.Runtime.InteropServices;
using Amazon.CognitoIdentityProvider;
using Amazon.Extensions.CognitoAuthentication;
using Amazon.S3;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDbGenericRepository;
using QuarklessContexts.Contexts;
using QuarklessContexts.Contexts.AccountContext;
using QuarklessContexts.InstaClient;
using QuarklessContexts.Models.Options;
using QuarklessContexts.Models.UserAuth.AuthTypes;
using QuarklessLogic.ContentSearch.GoogleSearch;
using QuarklessLogic.ContentSearch.SeleniumClient;
using QuarklessLogic.ContentSearch.YandexSearch;
using QuarklessLogic.Handlers.ClientProvider;
using QuarklessLogic.Handlers.EmailService;
using QuarklessLogic.Handlers.ReportHandler;
using QuarklessLogic.Handlers.RequestBuilder.RequestBuilder;
using QuarklessLogic.Handlers.RestSharpClient;
using QuarklessLogic.Handlers.TextGeneration;
using QuarklessLogic.Handlers.TranslateService;
using QuarklessLogic.Handlers.Util;
using QuarklessLogic.Handlers.WebHooks;
using QuarklessLogic.Logic.AccountLogic;
using QuarklessLogic.Logic.AuthLogic.Auth;
using QuarklessLogic.Logic.AuthLogic.Auth.Manager;
using QuarklessLogic.Logic.BusinessLogic;
using QuarklessLogic.Logic.CollectionsLogic;
using QuarklessLogic.Logic.CommentLogic;
using QuarklessLogic.Logic.DiscoverLogic;
using QuarklessLogic.Logic.HashtagLogic;
using QuarklessLogic.Logic.InstaAccountOptionsLogic;
using QuarklessLogic.Logic.InstagramAccountLogic;
using QuarklessLogic.Logic.InstaUserLogic;
using QuarklessLogic.Logic.LibaryLogic;
using QuarklessLogic.Logic.LookupLogic;
using QuarklessLogic.Logic.MediaLogic;
using QuarklessLogic.Logic.MessagingLogic;
using QuarklessLogic.Logic.ProfileLogic;
using QuarklessLogic.Logic.ProxyLogic;
using QuarklessLogic.Logic.QueryLogic;
using QuarklessLogic.Logic.ResponseLogic;
using QuarklessLogic.Logic.StorageLogic;
using QuarklessLogic.Logic.TimelineEventLogLogic;
using QuarklessLogic.QueueLogic.Services;
using QuarklessLogic.ServicesLogic;
using QuarklessLogic.ServicesLogic.AgentLogic;
using QuarklessLogic.ServicesLogic.ContentSearch;
using QuarklessLogic.ServicesLogic.CorpusLogic;
using QuarklessLogic.ServicesLogic.HeartbeatLogic;
using QuarklessLogic.ServicesLogic.TimelineServiceLogic.TimelineLogic;
using QuarklessRepositories.InstagramAccountRepository;
using QuarklessRepositories.ProfileRepository;
using QuarklessRepositories.ProxyRepository;
using QuarklessRepositories.RedisRepository.AccountCache;
using QuarklessRepositories.RedisRepository.APILogger;
using QuarklessRepositories.RedisRepository.CorpusCache.CommentCorpusCache;
using QuarklessRepositories.RedisRepository.CorpusCache.HashtagCorpusCache;
using QuarklessRepositories.RedisRepository.CorpusCache.MediaCorpusCache;
using QuarklessRepositories.RedisRepository.HeartBeaterRedis;
using QuarklessRepositories.RedisRepository.InstagramAccountRedis;
using QuarklessRepositories.RedisRepository.LibraryCache;
using QuarklessRepositories.RedisRepository.LoggerStoring;
using QuarklessRepositories.RedisRepository.LookupCache;
using QuarklessRepositories.RedisRepository.RedisClient;
using QuarklessRepositories.RedisRepository.SearchCache;
using QuarklessRepositories.RedisRepository.TimelineJobRedis;
using QuarklessRepositories.Repository.CorpusRepositories.Comments;
using QuarklessRepositories.Repository.CorpusRepositories.Medias;
using QuarklessRepositories.Repository.CorpusRepositories.Topic;
using QuarklessRepositories.Repository.LibraryRepository;
using QuarklessRepositories.Repository.ServicesRepositories.HashtagsRepository;
using QuarklessRepositories.Repository.ServicesRepositories.TopicsRepository;
using QuarklessRepositories.Repository.TimelineRepository;
using QuarklessRepositories.RepositoryClientManager;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Amazon;
using Amazon.CognitoIdentity;
using Amazon.Extensions.NETCore.Setup;
using AspNetCoreRateLimit;
using Quarkless.Analyser;
using Quarkless.Analyser.Models;
using Quarkless.Vision;
using QuarklessContexts.Models.APILogger;
using QuarklessContexts.Models.SecurityLayerModels;
using QuarklessLogic.Handlers.RequestBuilder.Constants;
using QuarklessLogic.Logic.TopicLookupLogic;
using QuarklessLogic.QueueLogic.Jobs.JobRunner;
using QuarklessLogic.ServicesLogic.TopicsServiceLogic;
using QuarklessRepositories.Repository.RepositoryClientManager;
using QuarklessRepositories.Repository.TopicLookupRepository;
using IpRateLimitPolicies = AspNetCoreRateLimit.IpRateLimitPolicies;
#endregion

namespace Quarkless.Common.Clients.Configs
{
	public static class ConfigureServices
	{
		private static readonly bool _isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
		public static void AddHangFrameworkServices(this IServiceCollection serviceCollection)
		{
			serviceCollection.AddTransient<ITaskService, TaskService>();
			serviceCollection.AddTransient<IBackgroundJobClient, BackgroundJobClient>();
			serviceCollection.AddTransient<IJobRunner, JobRunner>();
		}
		public static void AddLogicServices(this IServiceCollection services)
		{
			services.AddSingleton<IDiscoverLogic, DiscoverLogic>();
			services.AddTransient<IProxyLogic, ProxyLogic>();
			services.AddTransient<IInstagramAccountLogic, InstagramAccountLogic>();
			services.AddTransient<IProfileLogic, ProfileLogic>();
			services.AddTransient<IInstaUserLogic, InstaUserLogic>();
			services.AddTransient<ICommentLogic, CommentLogic>();
			services.AddTransient<ICollectionsLogic, CollectionsLogic>();
			services.AddTransient<IInstaAccountOptionsLogic, InstaAccountOptionsLogic>();
			services.AddTransient<IInstaClient, InstaClient>();
			services.AddTransient<IHashtagLogic, HashtagLogic>();
			services.AddTransient<IMediaLogic, MediaLogic>();
			services.AddTransient<ITimelineLogic, TimelineLogic>();
			services.AddTransient<IHeartbeatLogic, HeartbeatLogic>();
			services.AddTransient<IAgentLogic, AgentLogic>();
			services.AddTransient<ICommentCorpusLogic, CommentCorpusLogic>();
			services.AddTransient<IMediaCorpusLogic, MediaCorpusLogic>();
			services.AddTransient<IQueryLogic, QueryLogic>();
			services.AddSingleton<IContentSearcherHandler, ContentSearcherHandler>();
			services.AddTransient<ISearchingCache, SearchingCache>();
			services.AddTransient<ILibraryLogic, LibraryLogic>();
			services.AddTransient<ITimelineEventLogLogic, TimelineEventLogLogic>();
			services.AddTransient<IEmailService, EmailService>();
			services.AddTransient<IResponseResolver, ResponseResolver>();
			services.AddTransient<IBusinessLogic, BusinessLogic>();
			services.AddTransient<IMessagingLogic, MessagingLogic>();
			services.AddTransient<ILookupLogic, LookupLogic>();
			services.AddTransient<ILookupCache, LookupCache>();
			services.AddSingleton<IAccountLogic, AccountLogic>();
			services.AddSingleton<IWebHookHandlers, WebHookHandlers>();
			services.AddSingleton<IGoogleSearchLogic, GoogleSearchLogic>();
			services.AddSingleton<IYandexImageSearch, YandexImageSearch>();
			services.AddSingleton<IPostAnalyser, PostAnalyser>();
			services.AddSingleton<IMediaManipulation, MediaManipulation>();
			services.AddSingleton<ITopicLookupLogic, TopicLookupLogic>();
		}
		public static void AddAuthHandlers(this IServiceCollection services, EnvironmentsAccess accessors)
		{
			Environment.GetEnvironmentVariable("JWT_KEY");
			Environment.SetEnvironmentVariable("AWS_ACCESS_KEY_ID", accessors.AwsAccess.AccessKey);
			Environment.SetEnvironmentVariable("AWS_SECRET_ACCESS_KEY", accessors.AwsAccess.SecretKey);
			Environment.SetEnvironmentVariable("AWS_REGION", accessors.AwsAccess.Region);

			var regionEndpoint = RegionEndpoint.EUWest2;
			IAmazonCognitoIdentityProvider amazonCognitoIdentityProvider = new AmazonCognitoIdentityProviderClient(accessors.AwsAccess.AccessKey, accessors.AwsAccess.SecretKey, regionEndpoint);
			var userPool = new CognitoUserPool(accessors.AwsPool.PoolID, accessors.AwsPool.AppClientID, amazonCognitoIdentityProvider, accessors.AwsPool.AppClientSecret);
			services.AddSingleton(amazonCognitoIdentityProvider);
			services.AddSingleton(userPool);

			services.AddAWSService<IAmazonS3>();

			services.AddSingleton<IAmazonS3>(new AmazonS3Client(accessors.AwsAccess.AccessKey,
				accessors.AwsAccess.SecretKey, new AmazonS3Config
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
				ProfilesLocation = accessors.AwsOptions.ProfileLocation,
				Profile = accessors.AwsOptions.Profile
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

					o.Audience = accessors.AwsPool.AppClientID;
					o.Authority = accessors.AwsPool.AuthUrl;
					o.RequireHttpsMetadata = false;
					o.SaveToken = true;

				}
			);

			services.AddSingleton<IAuthAccessHandler>(new AuthAccessHandler(accessors.AwsPool.AppClientSecret));
			services.AddScoped<IAuthHandler, AuthHandler>();
			services.AddScoped<IAuthorizationHandler, AuthClientHandler>();
		}
		public static void AddRequestLogging(this IServiceCollection services, EnvironmentsAccess accessors)
		{
			if (accessors.IpRateLimiting != null)
			{
				services.Configure<IpRateLimitOptions>(options =>
				{
					options.ClientIdHeader = accessors.IpRateLimiting.ClientIdHeader;
					options.IpWhitelist = accessors.IpRateLimiting.IpWhitelist;
					options.EnableEndpointRateLimiting = accessors.IpRateLimiting.EnableEndpointRateLimiting;
					options.StackBlockedRequests = accessors.IpRateLimiting.StackBlockedRequests;
					options.RealIpHeader = accessors.IpRateLimiting.RealIpHeader;
					options.HttpStatusCode = accessors.IpRateLimiting.HttpStatusCode;
					options.EndpointWhitelist = accessors.IpRateLimiting.EndpointWhitelist;
					options.ClientWhitelist = accessors.IpRateLimiting.ClientWhitelist;
					options.GeneralRules = accessors.IpRateLimiting
						.GeneralRules.Select(x => new RateLimitRule
						{
							Endpoint = x.Endpoint,
							Limit = x.Limit,
							Period = x.Period,
						}).ToList();
				});
			}
			if (accessors.IpRateLimitPolicies != null)
			{
				services.Configure<IpRateLimitPolicies>(options =>
				{
					options.IpRules = accessors.IpRateLimitPolicies?.IpRules?.Select(x => new IpRateLimitPolicy
					{
						Ip = x.Ip,
						Rules = x.GeneralRules.Select(y => new RateLimitRule
						{
							Endpoint = y.Endpoint,
							Limit = y.Limit,
							Period = y.Period,
						}).ToList()
					}).ToList();
				});
			}
			if (accessors.MaxConcurrentRequests != null)
			{
				services.Configure<MaxConcurrentRequests>(options =>
				{
					options.Enabled = accessors.MaxConcurrentRequests.Enabled;
					options.ExcludePaths = accessors.MaxConcurrentRequests.ExcludePaths;
					options.Limit = accessors.MaxConcurrentRequests.Limit;
					options.LimitExceededPolicy = accessors.MaxConcurrentRequests.LimitExceededPolicy;
					options.MaxQueueLength = accessors.MaxConcurrentRequests.MaxQueueLength;
					options.MaxTimeInQueue = accessors.MaxConcurrentRequests.MaxTimeInQueue;
				});
			}
		}
		public static void AddConfigurators(this IServiceCollection services, EnvironmentsAccess accessors)
		{
			services.Configure<MongoSettings>(o => {
				o.ConnectionString = accessors.ConnectionString;
				o.MainDatabase = accessors.MainDatabase;
				o.ControlDatabase = accessors.ControlDatabase;
				o.ContentDatabase = accessors.ContentDatabase;
				o.SchedulerDatabase = accessors.SchedulerDatabase;
			});
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

			services.AddSingleton<IVideoEditor>(new VideoEditor(new MediaAnalyserOptions
			{
				TempImagePath = accessors.TempImagePath,
				TempVideoPath = accessors.TempVideoPath,
				FfmpegEnginePath = accessors.FfmpegPath,
				IsOnWindows = _isWindows
			}));
			services.AddSingleton<IUrlReader>(new UrlReader(accessors.ApiBasePath));

			services.AddSingleton<IVisionClient>(new VisionClient(accessors.VisionCredentials));
		}
		public static void AddRepositories(this IServiceCollection services)
		{
			BsonSerializer.RegisterSerializer(typeof(Guid),
			new GuidSerializer(BsonType.String));
			services.AddSingleton<IRepositoryContext, RepositoryContext>();
			services.AddTransient<IInstagramAccountRepository, InstagramAccountRepository>();
			services.AddTransient<IProxyRepostory, ProxyRepository>();
			services.AddTransient<IProfileRepository, ProfileRepository>();
			services.AddTransient<ITopicsRepository, TopicsRepository>();
			services.AddTransient<IHashtagsRepository, HashtagsRepository>();
			services.AddTransient<ITimelineLoggingRepository, TimelineLoggingRepository>();
			services.AddTransient<IHeartbeatRepository, HeartbeatRepository>();
			services.AddTransient<IInstagramAccountRedis, InstagramAccountRedis>();
			services.AddTransient<ICommentCorpusRepository, CommentCorpusRepository>();
			services.AddTransient<ICommentCorpusCache, CommentCorpusCache>();
			services.AddTransient<IMediaCorpusRepository, MediaCorpusRepository>();
			services.AddTransient<IHashtagCoprusCache, HashtagCoprusCache>();
			services.AddTransient<IMediaCorpusCache, MediaCorpusCache>();
			services.AddTransient<ITimelineJobRepository, TimelineJobRepository>();
			services.AddTransient<ITopicCategoryRepository, TopicCategoryRepository>();
			services.AddTransient<ILibraryRepository, LibraryRepository>();
			services.AddTransient<ILibraryCache, LibraryCache>();
			services.AddTransient<IAPILogCache, APILogCache>();
			services.AddTransient<ILoggerStore, LoggerStore>();
			services.AddTransient<IRedisClient, RedisClient>();
			services.AddTransient<IAccountCache, AccountCache>();
			services.AddTransient<ITopicLookupRepository, TopicLookupRepository>();
		}
		public static void AddHandlers(this IServiceCollection services)
		{
			services.AddTransient<IReportHandler, ReportHandler>();
			services.AddSingleton<IRestSharpClientManager, RestSharpClientManager>();
			services.AddTransient<ITopicServicesLogic, TopicServicesLogic>();

			services.AddTransient<ISeleniumClient, SeleniumClient>();

			services.AddTransient<IClientContextProvider, ClientContextProvider>();
			services.AddTransient<IAPIClientContext, APIClientContext>();
			services.AddTransient<IAPIClientContainer, APIClientContainer>();
			services.AddTransient<ITranslateService, TranslateService>();
			services.AddTransient<IUtilProviders, UtilProviders>();
			services.AddSingleton<ITextGeneration, TextGeneration>();
		}
		public static void AddContexts(this IServiceCollection services)
		{
			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
			services.AddTransient<IUserContext, UserContext>();
			services.AddTransient<IRequestBuilder, RequestBuilder>();
		}
	}
}
