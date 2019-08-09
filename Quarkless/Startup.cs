using System;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quarkless.Common;
using QuarklessLogic.QueueLogic.Jobs.Filters;
using StackExchange.Redis;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc.Cors.Internal;
using Quarkless.Filters;

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
			var Redis = ConnectionMultiplexer.Connect(_accessors.RedisConnectionString);

			services.AddAuthHandlers(_accessors, Configuration.GetAWSOptions());		
			services.AddHttpContextAccessor();
			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
			services.AddLogics();
			services.AddContexts();
			services.AddHandlers();
			services.AddHangFrameworkServices(_accessors);
			services.AddRepositories(_accessors);
			services.AddCors(options=>{
				options.AddPolicy(CorsPolicy,
					builder=>
					{
					//	builder.WithOrigins(_accessors.FrontEnd);
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

				options.UseRedisStorage(Redis, new Hangfire.Redis.RedisStorageOptions
				{
					Db = 1,
					Prefix = "Timeline",
					SucceededListSize = 1000,
					DeletedListSize = 1000,
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
			BackgroundJobServerOptions jobServerOptions = new BackgroundJobServerOptions
			{
				WorkerCount = Environment.ProcessorCount * 5,
				ServerName = string.Format("ISE_{0}.{1}", Environment.MachineName, Guid.NewGuid().ToString()),
				//Activator = new WorkerActivator(services.BuildServiceProvider(false)),
			};
			app.UseCors(CorsPolicy);
			app.UseHangfireServer(jobServerOptions);
			app.UseHangfireDashboard();
			app.UseHttpsRedirection();
			app.UseStaticFiles();	
			app.UseDefaultFiles();
			app.UseCookiePolicy();
			app.UseAuthentication();
			//app.UseMiddleware<RequestResponseLoggingMiddleware>();
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
