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
using Quarkless.Base.Account.Logic;
using Quarkless.Base.Account.Models;
using Quarkless.Base.Account.Models.Interfaces;
using Quarkless.Base.AccountOptions;
using Quarkless.Base.Actions.Logic.Factory.ActionExecute.Manager;
using Quarkless.Base.Actions.Models.Interfaces;
using Quarkless.Base.Agent.Logic;
using Quarkless.Base.Agent.Models.Interfaces;
using Quarkless.Base.ApiLogger.Models;
using Quarkless.Base.ApiLogger.Models.Interfaces;
using Quarkless.Base.ApiLogger.Repository;
using Quarkless.Base.Auth.Common.Models.AccountContext;
using Quarkless.Base.Auth.Common.Models.Enums;
using Quarkless.Base.Auth.Common.Models.Interfaces;
using Quarkless.Base.Auth.Logic;
using Quarkless.Base.Auth.Logic.Manager;
using Quarkless.Base.Auth.Models.Interfaces;
using Quarkless.Base.AuthDetails.Logic;
using Quarkless.Base.AuthDetails.Models.Interfaces;
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
using Quarkless.Base.InstagramBusiness;
using Quarkless.Base.InstagramClient.Logic;
using Quarkless.Base.InstagramClient.Models.Interfaces;
using Quarkless.Base.InstagramCollections;
using Quarkless.Base.InstagramComments.Logic;
using Quarkless.Base.InstagramComments.Models.Interfaces;
using Quarkless.Base.InstagramComments.Repository;
using Quarkless.Base.InstagramDiscover;
using Quarkless.Base.InstagramSearch.Logic;
using Quarkless.Base.InstagramSearch.Models.Interfaces;
using Quarkless.Base.InstagramUser;
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
using Quarkless.Base.Query.Logic;
using Quarkless.Base.Query.Models.Interfaces;
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
using Quarkless.Base.Timeline.Repository;
using Quarkless.Base.Topic.Logic;
using Quarkless.Base.Topic.Models.Interfaces;
using Quarkless.Base.Topic.Repository;
using Quarkless.Base.Utilities.Logic;
using Quarkless.Base.Utilities.Models.Interfaces;
using Quarkless.Base.WebHooks.Logic;
using Quarkless.Base.WebHooks.Models.Interfaces;
using Quarkless.Base.WorkerManager.Logic;
using Quarkless.Base.WorkerManager.Models.Interfaces;
using Quarkless.Events;
using Quarkless.Events.Interfaces;
using Quarkless.Events.Models;
using Quarkless.Filters;
using Quarkless.Geolocation;
using Quarkless.Models.Common.Extensions;
using Quarkless.Models.Shared.Api.Extensions;
using Quarkless.Models.Shared.Extensions;
using Quarkless.Repository.MongoContext;
using Quarkless.Repository.MongoContext.Models;
using Quarkless.Repository.RedisContext;
using Quarkless.Repository.RedisContext.Models;
using Quarkless.Vision;
using IpRateLimitPolicies = AspNetCoreRateLimit.IpRateLimitPolicies;
using RequestBuilder = Google.Apis.Requests.RequestBuilder;

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
			services.AddTransient<IRequestBuilder, Base.RequestBuilder.Logic.RequestBuilder>();
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
