using Amazon;
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
using QuarklessContexts.Contexts;
using QuarklessContexts.Contexts.AccountContext;
using QuarklessContexts.InstaClient;
using QuarklessContexts.JobClass;
using QuarklessContexts.Models.UserAuth.AuthTypes;
using QuarklessLogic.ContentSearch.SeleniumClient;
using QuarklessLogic.Handlers.ClientProvider;
using QuarklessLogic.Handlers.ReportHandler;
using QuarklessLogic.Handlers.RequestBuilder.RequestBuilder;
using QuarklessLogic.Handlers.RestSharpClient;
using QuarklessLogic.Handlers.TextGeneration;
using QuarklessLogic.Handlers.TranslateService;
using QuarklessLogic.Handlers.Util;
using QuarklessLogic.Logic.AuthLogic.Auth;
using QuarklessLogic.Logic.AuthLogic.Auth.Manager;
using QuarklessLogic.Logic.CollectionsLogic;
using QuarklessLogic.Logic.CommentLogic;
using QuarklessLogic.Logic.DiscoverLogic;
using QuarklessLogic.Logic.HashtagLogic;
using QuarklessLogic.Logic.InstaAccountOptionsLogic;
using QuarklessLogic.Logic.InstagramAccountLogic;
using QuarklessLogic.Logic.InstaUserLogic;
using QuarklessLogic.Logic.MediaLogic;
using QuarklessLogic.Logic.ProfileLogic;
using QuarklessLogic.Logic.ProxyLogic;
using QuarklessLogic.Logic.QueryLogic;
using QuarklessLogic.Logic.StorageLogic;
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
using QuarklessRepositories.RedisRepository.CorpusCache.CommentCorpusCache;
using QuarklessRepositories.RedisRepository.CorpusCache.HashtagCorpusCache;
using QuarklessRepositories.RedisRepository.CorpusCache.MediaCorpusCache;
using QuarklessRepositories.RedisRepository.HeartBeaterRedis;
using QuarklessRepositories.RedisRepository.InstagramAccountRedis;
using QuarklessRepositories.RedisRepository.RedisClient;
using QuarklessRepositories.RedisRepository.SearchCache;
using QuarklessRepositories.RedisRepository.TimelineJobRedis;
using QuarklessRepositories.Repository.CorpusRepositories.Comments;
using QuarklessRepositories.Repository.CorpusRepositories.Medias;
using QuarklessRepositories.Repository.CorpusRepositories.Topic;
using QuarklessRepositories.Repository.ServicesRepositories;
using QuarklessRepositories.Repository.ServicesRepositories.CommentsRepository;
using QuarklessRepositories.Repository.ServicesRepositories.HashtagsRepository;
using QuarklessRepositories.Repository.ServicesRepositories.TopicsRepository;
using QuarklessRepositories.Repository.TimelineRepository;
using QuarklessRepositories.RepositoryClientManager;
using System;
using System.IO;
using QuarklessContexts.Models.Options;
using QuarklessLogic.ContentSearch.GoogleSearch;
using QuarklessLogic.ContentSearch.YandexSearch;
using QuarklessLogic.Handlers.EmailService;
using QuarklessLogic.Handlers.WebHooks;
using QuarklessLogic.Logic.AccountLogic;
using QuarklessLogic.Logic.BusinessLogic;
using QuarklessLogic.Logic.LibaryLogic;
using QuarklessLogic.Logic.LookupLogic;
using QuarklessLogic.Logic.MessagingLogic;
using QuarklessLogic.Logic.ResponseLogic;
using QuarklessLogic.Logic.TimelineEventLogLogic;
using QuarklessRepositories.RedisRepository.APILogger;
using QuarklessRepositories.RedisRepository.LibraryCache;
using QuarklessRepositories.Repository.LibraryRepository;
using QuarklessRepositories.RedisRepository.LoggerStoring;
using QuarklessRepositories.RedisRepository.LookupCache;

namespace Quarkless.Common
{
	public static class ConfigureServices
	{
		public static void AddHangFrameworkServices(this IServiceCollection serviceCollection, Accessors accessors)
		{
			serviceCollection.AddTransient<ITaskService, TaskService>();
			serviceCollection.AddTransient<IBackgroundJobClient, BackgroundJobClient>();
			serviceCollection.AddTransient<IJobRunner, JobRunner>();		
		}
		public static void AddLogics(this IServiceCollection services)
		{
			services.AddSingleton<IDiscoverLogic, DiscoverLogic>();
			services.AddTransient<IProxyLogic, ProxyLogic>();
			services.AddTransient<IInstagramAccountLogic, InstagramAccountLogic>();
			services.AddTransient<IProfileLogic, ProfileLogic>();
			services.AddTransient<IInstaUserLogic, InstaUserLogic>();
			services.AddTransient<ICommentLogic, CommentLogic>();
			services.AddTransient<ICollectionsLogic, CollectionsLogic>();
			services.AddTransient<IInstaAccountOptionsLogic, InstaAccountOptionsLogic>();
			services.AddTransient<IInstaClient,InstaClient>();
			services.AddTransient<IHashtagLogic, HashtagLogic>();
			services.AddTransient<IMediaLogic,MediaLogic>();
			services.AddTransient<ITimelineLogic,TimelineLogic>();
			services.AddTransient<IHeartbeatLogic, HeartbeatLogic>();
			services.AddTransient<IAgentLogic,AgentLogic>();
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
		}
		public static void AddAuthHandlers(this IServiceCollection services, Accessors accessors, AWSOptions aWSOptions)
		{
			Environment.GetEnvironmentVariable("JWT_KEY");
			Environment.SetEnvironmentVariable("AWS_ACCESS_KEY_ID", accessors.AWSAccess.AccessKey);
			Environment.SetEnvironmentVariable("AWS_SECRET_ACCESS_KEY", accessors.AWSAccess.SecretKey);
			Environment.SetEnvironmentVariable("AWS_REGION", accessors.AWSAccess.Region);

			var regionEndpoint = RegionEndpoint.EUWest2;
			IAmazonCognitoIdentityProvider amazonCognitoIdentityProvider = new AmazonCognitoIdentityProviderClient(accessors.AWSAccess.AccessKey, accessors.AWSAccess.SecretKey, regionEndpoint);
			var userPool = new CognitoUserPool(accessors.AWSPool.PoolID, accessors.AWSPool.AppClientID, amazonCognitoIdentityProvider, accessors.AWSPool.AppClientSecret);
			services.AddSingleton(amazonCognitoIdentityProvider);
			services.AddSingleton(userPool);

			services.AddAWSService<IAmazonS3>();

			services.AddSingleton<IAmazonS3>(new AmazonS3Client(accessors.AWSAccess.AccessKey,
				accessors.AWSAccess.SecretKey, new AmazonS3Config
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

			services.AddDefaultAWSOptions(aWSOptions);

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

					o.Audience = accessors.AWSPool.AppClientID;
					o.Authority = accessors.AWSPool.AuthUrl;
					o.RequireHttpsMetadata = false;
					o.SaveToken = true;

					o.Events = new JwtBearerEvents
					{
						OnTokenValidated = async ctx =>
						{

						}
					};
				}
			);

			services.AddSingleton<IAuthAccessHandler>(new AuthAccessHandler(accessors.AWSPool.AppClientSecret));
			services.AddScoped<IAuthHandler, AuthHandler>();
			services.AddScoped<IAuthorizationHandler, AuthClientHandler>();
		}

		public static void AddConfigurators(this IServiceCollection services, Accessors accessors)
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
		}
		public static void AddRepositories(this IServiceCollection services)
		{
			BsonSerializer.RegisterSerializer(typeof(Guid),
			new GuidSerializer(BsonType.String));
			services.AddSingleton<IRepositoryContext, RepositoryContext>();
			services.AddTransient<IInstagramAccountRepository, InstagramAccountRepository>();
			services.AddTransient<IProxyRepostory, ProxyRepository>();
			services.AddTransient<IProfileRepository, ProfileRepository>();
			services.AddTransient<IPostServicesRepository, PostServicesRepository>();
			services.AddTransient<ITopicsRepository,TopicsRepository>();
			services.AddTransient<ICommentsRepository,CommentsRepository>();
			services.AddTransient<IHashtagsRepository,HashtagsRepository>();
			services.AddTransient<ITimelineLoggingRepository,TimelineLoggingRepository>();
			services.AddTransient<IHeartbeatRepository, HeartbeatRepository>();
			services.AddTransient<IInstagramAccountRedis,InstagramAccountRedis>();
			services.AddTransient<ICommentCorpusRepository, CommentCorpusRepository>();
			services.AddTransient<ICommentCorpusCache,CommentCorpusCache>();
			services.AddTransient<IMediaCorpusRepository,MediaCorpusRepository>();
			services.AddTransient<IHashtagCoprusCache, HashtagCoprusCache>();
			services.AddTransient<IMediaCorpusCache, MediaCorpusCache>();
			services.AddTransient<ITimelineJobRepository,TimelineJobRepository>();
			services.AddTransient<ITopicCategoryRepository, TopicCategoryRepository>();
			services.AddTransient<ILibraryRepository, LibraryRepository>();
			services.AddTransient<ILibraryCache, LibraryCache>();
			services.AddTransient<IAPILogCache, APILogCache>();
			services.AddTransient<ILoggerStore,LoggerStore>();
			services.AddTransient<IRedisClient,RedisClient>();
			services.AddTransient<IAccountCache, AccountCache>();
		}
		public static void AddHandlers(this IServiceCollection services)
		{
			services.AddTransient<IReportHandler, ReportHandler>();
			services.AddSingleton<IRestSharpClientManager, RestSharpClientManager>();
			services.AddTransient<ITopicServicesLogic, TopicServicesLogic>();

			services.Configure<SeleniumLaunchOptions>(options =>
				{
					options.ChromePath = Path.Combine(Accessors.BasePath, @"Requires\chrome");
				});
			services.AddTransient<ISeleniumClient, SeleniumClient>();

			services.AddTransient<IClientContextProvider, ClientContextProvider>();
			services.AddTransient<IAPIClientContext,APIClientContext>();
			services.AddTransient<IAPIClientContainer,APIClientContainer>();
			services.AddTransient<ITranslateService,TranslateService>();
			services.AddTransient<IUtilProviders,UtilProviders>();
			services.AddSingleton<ITextGeneration,TextGeneration>();
		}
		public static void AddContexts(this IServiceCollection services)
		{
			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
			services.AddTransient<IUserContext, UserContext>();
			services.AddTransient<IRequestBuilder,RequestBuilder>();
		}
	}
}
