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
using QuarklessLogic.Handlers.EmailService;
using QuarklessLogic.Logic.BusinessLogic;
using QuarklessLogic.Logic.LibaryLogic;
using QuarklessLogic.Logic.ResponseLogic;
using QuarklessLogic.Logic.TimelineEventLogLogic;
using QuarklessRepositories.RedisRepository.APILogger;
using QuarklessRepositories.RedisRepository.LibraryCache;

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
			services.AddTransient<IContentSearcherHandler, ContentSearcherHandler>();
			services.AddTransient<ISearchingCache, SearchingCache>();
			services.AddTransient<ILibraryLogic, LibraryLogic>();
			services.AddTransient<ITimelineEventLogLogic, TimelineEventLogLogic>();
			services.AddTransient<IEmailService, EmailService>();
			services.AddTransient<IResponseResolver, ResponseResolver>();
			services.AddTransient<IBusinessLogic, BusinessLogic>();
		}
		public static void AddAuthHandlers(this IServiceCollection services, Accessors _accessors, AWSOptions aWSOptions)
		{
			Environment.GetEnvironmentVariable("JWT_KEY");
			Environment.SetEnvironmentVariable("AWS_ACCESS_KEY_ID", _accessors.AWSAccess.AccessKey);
			Environment.SetEnvironmentVariable("AWS_SECRET_ACCESS_KEY", _accessors.AWSAccess.SecretKey);
			Environment.SetEnvironmentVariable("AWS_REGION", _accessors.AWSAccess.Region);

			RegionEndpoint regionEndpoint = RegionEndpoint.EUWest2;
			IAmazonCognitoIdentityProvider amazonCognitoIdentityProvider = new AmazonCognitoIdentityProviderClient(_accessors.AWSAccess.AccessKey, _accessors.AWSAccess.SecretKey, regionEndpoint);
			CognitoUserPool userPool = new CognitoUserPool(_accessors.AWSPool.PoolID, _accessors.AWSPool.AppClientID, amazonCognitoIdentityProvider, _accessors.AWSPool.AppClientSecret);
			services.AddSingleton(amazonCognitoIdentityProvider);
			services.AddSingleton(userPool);
			IAmazonS3 amazonS3 = new AmazonS3Client(_accessors.AWSAccess.AccessKey, _accessors.AWSAccess.SecretKey,new AmazonS3Config
			{
				RegionEndpoint = regionEndpoint,
				SignatureVersion = "v4",
				SignatureMethod = Amazon.Runtime.SigningAlgorithm.HmacSHA256
			});
			services.AddAWSService<IAmazonS3>();
			services.AddSingleton<IS3BucketLogic>(new S3BucketLogic(amazonS3));

			var mongoDbContext = new MongoDbContext(_accessors.ConnectionString, "Accounts");

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

					o.Audience = _accessors.AWSPool.AppClientID;
					o.Authority = _accessors.AWSPool.AuthUrl;
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


			services.AddSingleton<IAuthAccessHandler>(new AuthAccessHandler(_accessors.AWSPool.AppClientSecret));
			services.AddScoped<IAuthHandler, AuthHandler>();
			services.AddScoped<IAuthorizationHandler, AuthClientHandler>();
		}
		public static void AddRepositories(this IServiceCollection services, Accessors _accessors)
		{
			BsonSerializer.RegisterSerializer(typeof(Guid),
			new GuidSerializer(BsonType.String));
			services.Configure<Settings>(o => {
				o.ConnectionString = _accessors.ConnectionString;
				o.MainDatabase = _accessors.MainDatabase;
				o.ControlDatabase = _accessors.ControlDatabase;
				o.ContentDatabase = _accessors.ContentDatabase;
				o.SchedulerDatabase = _accessors.SchedulerDatabase;
			});
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
			services.AddTransient<ILibraryCache, LibraryCache>();
			services.AddTransient<IAPILogCache, APILogCache>();
			services.Configure<RedisOptions>(o =>
			{
				o.ConnectionString = _accessors.RedisConnectionString;
				o.DefaultKeyExpiry = TimeSpan.FromDays(7);
			});
			services.Configure<TranslateOptions>(options =>
			{
				options.DetectLangAPIKey =_accessors.DetectAPI;
				options.YandexAPIKey = _accessors.YandexAPIKey;
			});
			services.AddTransient<IRedisClient,RedisClient>();
			services.AddTransient<IAccountCache, AccountCache>();
		}
		public static void AddHandlers(this IServiceCollection services)
		{
			services.AddTransient<IReportHandler, ReportHandler>();
			services.AddSingleton<IRestSharpClientManager, RestSharpClientManager>();
			services.AddTransient<ITopicServicesLogic, TopicServicesLogic>();

			services.AddTransient<IClientContextProvider, ClientContextProvider>();
			services.AddTransient<IAPIClientContext,APIClientContext>();
			services.AddTransient<IAPIClientContainer,APIClientContainer>();
			services.AddTransient<ISeleniumClient,SeleniumClient>();
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
