using System;
using AspNetCoreRateLimit;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuarklessLogic.QueueLogic.Jobs.Filters;
using StackExchange.Redis;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Quarkless.Filters;
using QuarklessContexts.Models.SecurityLayerModels;
using Quarkless.Common.Clients;
using QuarklessContexts.Extensions;

namespace Quarkless
{
	public class Startup
    {
	    private const string CORS_POLICY = "HashtagGrowCORSPolicy";
	    private const string SERVER_IP = "security.quark";
		private const string CLIENT_SECTION = "Client";
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public static IConfiguration Configuration { get; private set; }

        public void ConfigureServices(IServiceCollection services)
        {
			//var hosts = Dns.GetHostEntry("quarkless.security.transport");
	        var cIn = new ClientRequester(SERVER_IP);
	        if (!cIn.TryConnect().GetAwaiter().GetResult())
		        return;

	        var caller = Configuration.GetSection(CLIENT_SECTION).Get<AvailableClient>();
	        var validate = cIn.Send(new InitCommandArgs
	        {
		        Client = caller,
		        CommandName = "Validate Client"
	        });
	        if (!(bool)validate)
		        return;
	        
	        var servicesAfar = (IServiceCollection) cIn.Send(new BuildCommandArgs()
	        {
		        CommandName = "Build Services",
		        ServiceTypes = new[]
		        {
			        ServiceTypes.AddAuthHandlers,
			        ServiceTypes.AddConfigurators,
			        ServiceTypes.AddContexts,
			        ServiceTypes.AddHandlers,
			        ServiceTypes.AddHangFrameworkServices,
			        ServiceTypes.AddLogics,
			        ServiceTypes.AddRepositories,
			        ServiceTypes.AddRequestLogging
		        }
	        });

	        services.Append(servicesAfar);

	        var endPoints = (EndPoints) cIn.Send(new GetPublicEndpointCommandArgs
	        {
		        CommandName = "Get Public Endpoints",
	        });
	        var redis = ConnectionMultiplexer.Connect(endPoints.RedisCon);

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
			services.AddCors(options=>{
				options.AddPolicy(CORS_POLICY,
					builder=>
					{
						builder.WithOrigins(endPoints.FrontEnd);
						builder.AllowAnyOrigin();
						builder.AllowAnyHeader();
						builder.AllowAnyMethod();
					});
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
			var jobServerOptions = new BackgroundJobServerOptions
			{
				WorkerCount = Environment.ProcessorCount * 5,
				ServerName = $"ISE_{Environment.MachineName}.{Guid.NewGuid().ToString()}",
				//Activator = new WorkerActivator(services.BuildServiceProvider(false)),
			};

			app.UseHttpsRedirection();
			app.UseRouting();
			//app.UseIpRateLimiting();

			app.UseCors(CORS_POLICY);
			app.UseHangfireServer(jobServerOptions);
			app.UseHangfireDashboard("/configs/hangfire", new DashboardOptions
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
			//	app.UseSecurityMiddleware(new SecurityHeadersBuilder()
			//		.AddDefaultSecurePolicy()
			//	/*.AddCustomHeader("X-Client-ID", "x-key-1-v")*/);
			//app.UseMvc();
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
