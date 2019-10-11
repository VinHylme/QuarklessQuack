using System;
using AspNetCoreRateLimit;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quarkless.Common;
using QuarklessLogic.QueueLogic.Jobs.Filters;
using StackExchange.Redis;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Cors.Internal;
using Quarkless.Filters;
using Quarkless.Extensions;
namespace Quarkless
{
	public class Startup
    {
		private readonly Accessors _accessors;
		private const string CorsPolicy = "HashtagGrowCORSPolicy";
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
			_accessors = new Accessors(configuration);
		}

        public static IConfiguration Configuration { get; private set; }

        public void ConfigureServices(IServiceCollection services)
        {
			var redis = ConnectionMultiplexer.Connect(_accessors.RedisConnectionString);
			// needed to load configuration from appsettings.json
			services.AddOptions();

			// needed to store rate limit counters and ip rules
			services.AddMemoryCache();

			//load general configuration from appsettings.json
			services.Configure<IpRateLimitOptions>(Configuration.GetSection("IpRateLimiting"));

			//load ip rules from appsettings.json
			services.Configure<IpRateLimitPolicies>(Configuration.GetSection("IpRateLimitPolicies"));

			//// inject counter and rules stores
			//services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
			//services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
			// inject counter and rules distributed cache stores

			services.AddSingleton<IIpPolicyStore, DistributedCacheIpPolicyStore>();
			services.AddSingleton<IRateLimitCounterStore,DistributedCacheRateLimitCounterStore>();
			services.Configure<CookiePolicyOptions>(options =>
			{
				// This lambda determines whether user consent for non-essential cookies is needed for a given request.
				options.CheckConsentNeeded = context => true;
				options.MinimumSameSitePolicy = SameSiteMode.None;
			});
			services.ConfigureRequestThrottleServices(Configuration);
			services.AddConfigurators(_accessors);
			services.AddAuthHandlers(_accessors, Configuration.GetAWSOptions());		
			services.AddHttpContextAccessor();
			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

			// https://github.com/aspnet/Hosting/issues/793
			// the IHttpContextAccessor service is not registered by default.
			// the clientId/clientIp resolvers use it.
			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

			// configuration (resolvers, counter key builders)
			services.AddSingleton<IRateLimitConfiguration, CustomRateLimitConfiguration>();

			services.AddLogics();
			services.AddContexts();
			services.AddHandlers();
			services.AddHangFrameworkServices(_accessors);
			services.AddRepositories();
			services.AddCors(options=>{
				options.AddPolicy(CorsPolicy,
					builder=>
					{
						builder.WithOrigins(_accessors.FrontEnd);
						builder.AllowAnyOrigin();
						builder.AllowAnyHeader();
						builder.AllowAnyMethod();
					});//.WithOrigins(_accessors.FrontEnd));
			});
			services.Configure<MvcOptions>(options =>
			{
				options.Filters.Add(new CorsAuthorizationFilterFactory(CorsPolicy));
			});
			services.AddHangfire(options =>
			{
			//	options.UseFilter(new ProlongExpirationTimeAttribute());
				#region Mongo Storage
				//options.UseMongoStorage(_accessors.ConnectionString, _accessors.SchedulerDatabase, new MongoStorageOptions()
				//{
				//	Prefix = "Timeline",
				//	CheckConnection = false,
				//	MigrationOptions = new MongoMigrationOptions(MongoMigrationStrategy.Migrate) 
				//	{ 
				//		Strategy = MongoMigrationStrategy.Migrate,
				//		BackupStrategy = MongoBackupStrategy.Collections
				//	},
				//	QueuePollInterval = TimeSpan.FromSeconds(2),
				//});
				#endregion
				options.UseRedisStorage(redis, new Hangfire.Redis.RedisStorageOptions
				{
					Db = 1,
					Prefix = "Timeline",
					SucceededListSize = 100000,
					DeletedListSize = 100000,
					ExpiryCheckInterval = TimeSpan.FromHours(1),
					InvisibilityTimeout = TimeSpan.FromMinutes(30),
					FetchTimeout = TimeSpan.FromMinutes(30),
					UseTransactions = true
				});
			});

			GlobalConfiguration.Configuration.UseActivator(new WorkerActivator(services.BuildServiceProvider(false)));
			GlobalConfiguration.Configuration.UseSerializerSettings(new Newtonsoft.Json.JsonSerializerSettings()
			{
				ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
			});
			GlobalJobFilters.Filters.Add(new ProlongExpirationTimeAttribute());

		}

		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }
			var jobServerOptions = new BackgroundJobServerOptions
			{
				WorkerCount = Environment.ProcessorCount * 5,
				ServerName = $"ISE_{Environment.MachineName}.{Guid.NewGuid().ToString()}",
				//Activator = new WorkerActivator(services.BuildServiceProvider(false)),
			};
			
			//app.UseIpRateLimiting();
			app.UseCors(CorsPolicy);
			app.UseHangfireServer(jobServerOptions);
			app.UseHangfireDashboard();
			app.UseHttpsRedirection();
			app.UseStaticFiles();	
			app.UseDefaultFiles();
			app.UseCookiePolicy();
			app.UseAuthentication();
			app.UseSecurityMiddleware(new SecurityHeadersBuilder()
				.AddDefaultSecurePolicy()
				/*.AddCustomHeader("X-Client-ID", "x-key-1-v")*/);
			app.UseMvc();
		}
	}
	public class WorkerActivator : JobActivator
	{
		private readonly IServiceProvider _serviceProvider;
		public WorkerActivator(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}
		public override object ActivateJob(Type jobType) => _serviceProvider.GetService(jobType);
	}
}
