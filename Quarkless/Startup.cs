using System;
using AspNetCoreRateLimit;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Quarkless.Extensions;
using Quarkless.Filters;
using Quarkless.Models.Timeline.TaskScheduler;
using Quarkless.Models.Shared.Extensions;

namespace Quarkless
{
	public class Startup
    {
	    private const string CORS_POLICY = "HashtagGrowCORSPolicy";
	    private const string REDIS_DB_NAME = "Timeline";

	    public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public static IConfiguration Configuration { get; private set; }

        public void ConfigureServices(IServiceCollection services)
        {
			
			#region Add Services
			services.IncludeHangFrameworkServices();
			services.IncludeLogicServices();
			services.IncludeAuthHandlers();
			services.IncludeRequestLogging();
			services.IncludeConfigurators();
			services.IncludeRepositories();
			services.IncludeHandlers();
			services.IncludeContexts();
			services.IncludeEventServices();

			services.AddControllers();
	        services.AddOptions();
			services.AddMemoryCache();
			services.AddSingleton<IIpPolicyStore, DistributedCacheIpPolicyStore>();
			services.AddSingleton<IRateLimitCounterStore,DistributedCacheRateLimitCounterStore>();
			services.Configure<CookiePolicyOptions>(options =>
			{
				options.CheckConsentNeeded = context => true;
				options.MinimumSameSitePolicy = SameSiteMode.None;
			});
			services.AddHttpContextAccessor();
			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
			services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
			services.AddSingleton<IRateLimitConfiguration, CustomRateLimitConfiguration>();

			var environments = new Config().Environments;
			var redis = ConnectionMultiplexer.Connect(environments.RedisConnectionString);

			services.AddCors(options=>{
				options.AddPolicy(CORS_POLICY,
					builder=>
					{
						//builder.WithOrigins(environments.FrontEnd);
						builder.SetIsOriginAllowed(_ => true);
						builder.AllowAnyOrigin();
						builder.AllowAnyHeader();
						builder.AllowAnyMethod();
					});
			});

			services.AddHangfire(options =>
			{
				options.UseRedisStorage(redis, new Hangfire.Redis.RedisStorageOptions
				{
					Db = 1,
					Prefix = REDIS_DB_NAME,
					SucceededListSize = 100000,
					DeletedListSize = 100000,
					ExpiryCheckInterval = TimeSpan.FromHours(1),
					InvisibilityTimeout = TimeSpan.FromMinutes(30),
					FetchTimeout = TimeSpan.FromMinutes(30),
					UseTransactions = true
				});
			});
			#endregion

			var serializerSettings = new JsonSerializerSettings()
			{
				ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
			};

			GlobalConfiguration.Configuration.UseSerializerSettings(serializerSettings);
			GlobalJobFilters.Filters.Add(new ProlongExpirationTimeAttribute());
        }

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            GlobalConfiguration.Configuration.UseActivator(new WorkerActivator(app.ApplicationServices));

            var jobServerOptions = new BackgroundJobServerOptions
			{
				WorkerCount = Environment.ProcessorCount * 5,
				ServerName = $"ISE_{Environment.MachineName}.{Guid.NewGuid().ToString()}",
			};

			app.UseHttpsRedirection();
			app.UseRouting();
			app.UseIpRateLimiting();

			app.UseCors(CORS_POLICY);
			app.UseHangfireServer(jobServerOptions);
			app.UseHangfireDashboard("/configs/hang-fire", new DashboardOptions
			{
				Authorization = new[] { new HangfireAllowAllConnectionsFilter() }
			});
			app.UseStaticFiles();	
			app.UseDefaultFiles();
			app.UseCookiePolicy();
			app.UseAuthentication();
			app.UseAuthorization();
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});

			app.UseSecurityMiddleware(new SecurityHeadersBuilder()
				.AddDefaultSecurePolicy()
				.AddCustomHeader("X-Client-ID", "x-key-1-v"));
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
