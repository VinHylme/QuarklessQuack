using System;
using System.Linq;
using System.Runtime.InteropServices;
using Amazon;
using Amazon.CognitoIdentity;
using Amazon.CognitoIdentityProvider;
using Amazon.Extensions.CognitoAuthentication;
using Amazon.Extensions.NETCore.Setup;
using Amazon.S3;
using AspNetCoreRateLimit;
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
using Quarkless.Base.AccountOptions;
using Quarkless.Base.ContentSearch;
using Quarkless.Base.InstagramBusiness;
using Quarkless.Base.InstagramCollections;
using Quarkless.Base.InstagramComments;
using Quarkless.Base.InstagramDiscover;
using Quarkless.Base.InstagramUser;
using Quarkless.Events;
using Quarkless.Events.Interfaces;
using Quarkless.Filters;
using Quarkless.Geolocation;
using Quarkless.Logic.Account;
using Quarkless.Logic.Actions.Factory.ActionExecute.Manager;
using Quarkless.Logic.Agent;
using Quarkless.Logic.Auth;
using Quarkless.Logic.Auth.Manager;
using Quarkless.Logic.Comments;
using Quarkless.Logic.ContentInfo;
using Quarkless.Logic.ContentSearch;
using Quarkless.Logic.Details;
using Quarkless.Logic.Hashtag;
using Quarkless.Logic.HashtagGenerator;
using Quarkless.Logic.Heartbeat;
using Quarkless.Logic.InstagramAccounts;
using Quarkless.Logic.InstagramClient;
using Quarkless.Logic.InstagramSearch;
using Quarkless.Logic.Library;
using Quarkless.Logic.Lookup;
using Quarkless.Logic.Media;
using Quarkless.Logic.Messaging;
using Quarkless.Logic.Notification;
using Quarkless.Logic.Profile;
using Quarkless.Logic.Proxy;
using Quarkless.Logic.PuppeteerClient;
using Quarkless.Logic.Query;
using Quarkless.Logic.ReportHandler;
using Quarkless.Logic.RequestBuilder;
using Quarkless.Logic.ResponseResolver;
using Quarkless.Logic.RestSharpClientManager;
using Quarkless.Logic.Storage;
using Quarkless.Logic.TextGenerator;
using Quarkless.Logic.Timeline;
using Quarkless.Logic.Timeline.TaskScheduler;
using Quarkless.Logic.Topic;
using Quarkless.Logic.Utilities;
using Quarkless.Logic.WebHooks;
using Quarkless.Logic.WorkerManager;
using Quarkless.Models.Account;
using Quarkless.Models.Account.Interfaces;
using Quarkless.Models.Actions.Interfaces;
using Quarkless.Models.Agent.Interfaces;
using Quarkless.Models.ApiLogger;
using Quarkless.Models.ApiLogger.Interfaces;
using Quarkless.Models.Auth;
using Quarkless.Models.Auth.AccountContext;
using Quarkless.Models.Auth.Enums;
using Quarkless.Models.Auth.Interfaces;
using Quarkless.Models.Comments.Interfaces;
using Quarkless.Models.Common.Extensions;
using Quarkless.Models.ContentInfo.Interfaces;
using Quarkless.Models.ContentSearch.Interfaces;
using Quarkless.Models.Hashtag.Interfaces;
using Quarkless.Models.HashtagGenerator.Interfaces;
using Quarkless.Models.Heartbeat.Interfaces;
using Quarkless.Models.InstagramAccounts;
using Quarkless.Models.InstagramAccounts.Interfaces;
using Quarkless.Models.InstagramClient.Interfaces;
using Quarkless.Models.InstagramSearch.Interfaces;
using Quarkless.Models.Library.Interfaces;
using Quarkless.Models.Lookup.Interfaces;
using Quarkless.Models.Media.Interfaces;
using Quarkless.Models.Messaging.Interfaces;
using Quarkless.Models.Notification.Interfaces;
using Quarkless.Models.Profile;
using Quarkless.Models.Profile.Interfaces;
using Quarkless.Models.Proxy;
using Quarkless.Models.Proxy.Interfaces;
using Quarkless.Models.Query.Interfaces;
using Quarkless.Models.ReportHandler.Interfaces;
using Quarkless.Models.RequestBuilder.Interfaces;
using Quarkless.Models.ResponseResolver.Interfaces;
using Quarkless.Models.RestSharpClientManager.Interfaces;
using Quarkless.Models.Shared.Api.Extensions;
using Quarkless.Models.Shared.Extensions;
using Quarkless.Models.Storage;
using Quarkless.Models.Storage.Interfaces;
using Quarkless.Models.TextGenerator.Interfaces;
using Quarkless.Models.Timeline.Interfaces;
using Quarkless.Models.Timeline.Interfaces.TaskScheduler;
using Quarkless.Models.Topic.Interfaces;
using Quarkless.Models.Utilities.Interfaces;
using Quarkless.Models.WebHooks.Interfaces;
using Quarkless.Models.WorkerManager.Interfaces;
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
using Quarkless.Repository.Notification;
using Quarkless.Repository.Profile;
using Quarkless.Repository.Proxy;
using Quarkless.Repository.RedisContext;
using Quarkless.Repository.RedisContext.Models;
using Quarkless.Repository.ReportHandler;
using Quarkless.Repository.Timeline;
using Quarkless.Repository.Topic;
using Quarkless.Vision;
using IpRateLimitPolicies = AspNetCoreRateLimit.IpRateLimitPolicies;

namespace Quarkless.Extensions
{
	internal static class ConfigurationServices
	{
		private static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
		internal static void IncludeHangFrameworkServices(this IServiceCollection serviceCollection)
		{
			serviceCollection.AddTransient<ITaskService, TaskService>();
			serviceCollection.AddTransient<IBackgroundJobClient, BackgroundJobClient>();
			serviceCollection.AddTransient<IJobRunner, JobRunner>();
			serviceCollection.AddTransient<IActionExecuteFactory, ActionExecuteFactoryManager>();
		}
		internal static void IncludeLogicServices(this IServiceCollection services)
		{
			services.AddSingleton<IDiscoverLogic, DiscoverLogic>();
			services.AddTransient<IProxyLogic, ProxyLogic>();
			services.AddTransient<IInstagramAccountLogic, InstagramAccountLogic>();
			services.AddTransient<IProfileLogic, ProfileLogic>();
			services.AddTransient<IInstaUserLogic, InstaUserLogic>();
			services.AddTransient<ICommentLogic, CommentLogic>();
			services.AddTransient<ICollectionsLogic, CollectionsLogic>();
			services.AddTransient<IInstaAccountOptionsLogic, InstaAccountOptionsLogic>();
			services.AddTransient<IHashtagLogic, HashtagLogic>();
			services.AddTransient<IMediaLogic, MediaLogic>();
			services.AddTransient<ITimelineLogic, TimelineLogic>();
			services.AddTransient<IHeartbeatLogic, HeartbeatLogic>();
			services.AddTransient<IAgentLogic, AgentLogic>();
			services.AddTransient<ICommentCorpusLogic, CommentCorpusLogic>();
			services.AddTransient<IMediaCorpusLogic, MediaCorpusLogic>();
			services.AddTransient<IQueryLogic, QueryLogic>();
			services.AddSingleton<IInstagramContentSearch, InstagramContentSearch>();
			services.AddTransient<ILibraryLogic, LibraryLogic>();
			services.AddTransient<INotificationLogic, NotificationLogic>();
			services.AddTransient<IResponseResolver, ResponseResolver>();
			services.AddTransient<IBusinessLogic, BusinessLogic>();
			services.AddTransient<IMessagingLogic, MessagingLogic>();
			services.AddTransient<ILookupLogic, LookupLogic>();
			services.AddTransient<ILookupCache, LookupCache>();
			services.AddSingleton<IWebHookHandler, StripeWebHookHandler>();
			services.AddSingleton<IGoogleSearchLogic, GoogleSearchLogic>();
			services.AddSingleton<IYandexImageSearch, YandexImageSearch>();
			services.AddSingleton<IPostAnalyser, PostAnalyser>();
			services.AddSingleton<IMediaManipulation, MediaManipulation>();
			services.AddSingleton<ITopicLookupLogic, TopicLookupLogic>();
			services.AddSingleton<ISearchProvider, SearchProvider>();
			services.AddTransient<IAccountDetailLogic, AccountDetailLogic>();
		}
		internal static void IncludeAuthHandlers(this IServiceCollection services)
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
				auth.DefaultPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
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
		internal static void IncludeRequestLogging(this IServiceCollection services)
		{
			var apiAccessors = new Configuration().Environments;
			if (apiAccessors.IpRateLimiting != null)
			{
				services.Configure<IpRateLimitOptions>(options =>
				{
					options.ClientIdHeader = apiAccessors.IpRateLimiting.ClientIdHeader;
					options.IpWhitelist = apiAccessors.IpRateLimiting.IpWhitelist;
					options.EnableEndpointRateLimiting = apiAccessors.IpRateLimiting.EnableEndpointRateLimiting;
					options.StackBlockedRequests = apiAccessors.IpRateLimiting.StackBlockedRequests;
					options.RealIpHeader = apiAccessors.IpRateLimiting.RealIpHeader;
					options.HttpStatusCode = apiAccessors.IpRateLimiting.HttpStatusCode;
					options.EndpointWhitelist = apiAccessors.IpRateLimiting.EndpointWhitelist;
					options.ClientWhitelist = apiAccessors.IpRateLimiting.ClientWhitelist;
					options.GeneralRules = apiAccessors.IpRateLimiting
						.GeneralRules.Select(x => new RateLimitRule
						{
							Endpoint = x.Endpoint,
							Limit = x.Limit,
							Period = x.Period,
						}).ToList();
				});
			}
			if (apiAccessors.IpRateLimitPolicies != null)
			{
				services.Configure<IpRateLimitPolicies>(options =>
				{
					options.IpRules = apiAccessors.IpRateLimitPolicies?.IpRules?.Select(x => new IpRateLimitPolicy
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
			if (apiAccessors.MaxConcurrentRequests != null)
			{
				services.Configure<MaxConcurrentRequests>(options =>
				{
					options.Enabled = apiAccessors.MaxConcurrentRequests.Enabled;
					options.ExcludePaths = apiAccessors.MaxConcurrentRequests.ExcludePaths;
					options.Limit = apiAccessors.MaxConcurrentRequests.Limit;
					options.LimitExceededPolicy = apiAccessors.MaxConcurrentRequests.LimitExceededPolicy;
					options.MaxQueueLength = apiAccessors.MaxConcurrentRequests.MaxQueueLength;
					options.MaxTimeInQueue = apiAccessors.MaxConcurrentRequests.MaxTimeInQueue;
				});
			}
		}
		internal static void IncludeConfigurators(this IServiceCollection services)
		{
			var accessors = new Config().Environments;
			var apiAccessors = new Configuration().Environments;
			services.Configure<S3Options>(options => { options.BucketName = accessors.S3BucketName; });
			services.Configure<RedisOptions>(o =>
			{
				o.ConnectionString = accessors.RedisConnectionString;
				o.DefaultKeyExpiry = TimeSpan.FromDays(7);
			});
			services.Configure<MaxConcurrentRequestsOptions>(options =>
			{
				options.Limit = apiAccessors.MaxConcurrentRequests.Limit;
				options.Enabled = apiAccessors.MaxConcurrentRequests.Enabled;
				options.ExcludePaths = apiAccessors.MaxConcurrentRequests.ExcludePaths;
				options.LimitExceededPolicy = apiAccessors.MaxConcurrentRequests.LimitExceededPolicy.GetValueFromDescription<MaxConcurrentRequestsLimitExceededPolicy>();
				options.MaxQueueLength = apiAccessors.MaxConcurrentRequests.MaxQueueLength;
				options.MaxTimeInQueue = apiAccessors.MaxConcurrentRequests.MaxTimeInQueue;
			});

			var stripeCredentials = JsonConvert.DeserializeObject<StripeCredentials>(accessors.JsonStripeCredentials);

			services.AddSingleton<IAccountLogic, AccountLogic>(s =>
				new AccountLogic(new AccountOptions
				{
					StripeKey = stripeCredentials
				}));

			services.AddSingleton<IFfmpegWrapper>(new FfmpegWrapper(new MediaAnalyserOptions
			{
				TempAudioPath = accessors.TempAudioPath,
				TempImagePath = accessors.TempImagePath,
				TempVideoPath = accessors.TempVideoPath,
				FfmpegEnginePath = accessors.FfmpegPath,
				IsOnWindows = IsWindows
			}));

			services.AddSingleton<IVideoEditor, VideoEditor>();
			services.AddSingleton<IAudioEditor, AudioEditor>();

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

			services.AddSingleton<IUrlReader>(new UrlReader(accessors.ApiBasePath));

			services.AddTransient<ISearchingCache, SearchingCache>();

			services.AddSingleton<IVisionClient, VisionClient>
				(s => new VisionClient(accessors.VisionCredentials, s.GetService<ISearchingCache>()));

			services.AddSingleton<IPuppeteerClient, PuppeteerClient>(s=> new PuppeteerClient(8));

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
			BsonSerializer.RegisterSerializer(typeof(Guid),
			new GuidSerializer(BsonType.String));
			services.AddTransient<IInstagramAccountRepository, InstagramAccountRepository>();
			services.AddTransient<IProxyAssignmentsRepository, ProxyAssignmentsRepository>();
			services.AddTransient<IProfileRepository, ProfileRepository>();
			services.AddTransient<IHashtagsRepository, HashtagsRepository>();
			services.AddTransient<INotificationRepository, NotificationRepository>();
			services.AddTransient<IHeartbeatRepository, HeartbeatRepository>();
			services.AddTransient<IInstagramAccountRedis, InstagramAccountRedis>();
			services.AddTransient<ICommentCorpusRepository, CommentCorpusRepository>();
			services.AddTransient<IMediaCorpusRepository, MediaCorpusRepository>();
			services.AddTransient<IActionExecuteLogsRepository, ActionExecuteLogsRepository>();
			services.AddTransient<ILibraryRepository, LibraryRepository>();
			services.AddTransient<ILibraryCache, LibraryCache>();
			services.AddTransient<IApiLogCache, ApiLogCache>();
			services.AddTransient<ILoggerStore, LoggerStore>();
			services.AddTransient<IRedisClient, RedisClient>();
			services.AddTransient<IAccountCache, AccountCache>();
			services.AddTransient<ITopicLookupRepository, TopicLookupRepository>();
			services.AddTransient<IReportHandlerRepository, ReportHandlerRepository>();
			services.AddTransient<IAccountRepository, AccountRepository>();
		}
		internal static void IncludeHandlers(this IServiceCollection services)
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
			services.AddSingleton<IWorkerManager, WorkerManager>();
		}
		internal static void IncludeContexts(this IServiceCollection services)
		{
			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
			services.AddTransient<IUserContext, UserContext>();
			services.AddTransient<IRequestBuilder, RequestBuilder>();
		}
		internal static void IncludeEventServices(this IServiceCollection services)
		{
			var accessors = new Config().Environments;

			services.AddTransient<IEventSubscriber<InstagramAccountPublishEventModel>, ProfileLogic>();
			services.AddTransient<IEventSubscriber<InstagramAccountDeletePublishEvent>, ProfileLogic>();
			services.AddTransient<IEventSubscriber<ProfileTopicAddRequest>, TopicLookupLogic>();

			services.AddTransient<IEventSubscriber<ProfilePublishEventModel>, ProxyRequest>(
				s => new ProxyRequest(
					new ProxyRequestOptions(accessors.ProxyHandlerApiEndPoint),
					s.GetService<IGeoLocationHandler>(),
					s.GetService<IProxyAssignmentsRepository>()));
			
			services.AddTransient<IEventSubscriber<ProfileDeletedEventModel>, ProxyRequest>(
				s => new ProxyRequest(
					new ProxyRequestOptions(accessors.ProxyHandlerApiEndPoint),
					s.GetService<IGeoLocationHandler>(),
					s.GetService<IProxyAssignmentsRepository>()));

			services.AddTransient<IEventPublisher, EventPublisher>(
				s => new EventPublisher(services));
		}
	}
}
