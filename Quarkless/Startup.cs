using System;
using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.Extensions.CognitoAuthentication;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDbGenericRepository;
using Quarkless.Common;
using Quarkless.Queue.Jobs.Filters;
using QuarklessContexts.Contexts.AccountContext;
using QuarklessContexts.Models.UserAuth.AuthTypes;
using QuarklessLogic.Logic.AuthLogic.Auth;
using QuarklessLogic.Logic.AuthLogic.Auth.Manager;
using Quartz;
using StackExchange.Redis;

namespace Quarkless
{
	public class WorkerActivator : JobActivator
	{
		private readonly IServiceProvider _serviceProvider;

		public WorkerActivator(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}
		public override object ActivateJob(Type jobType) => _serviceProvider.GetService(jobType);
	}
	public class Startup
    {
        //public const string AppS3BucketKey = "AppS3Bucket";	
		public static ConnectionMultiplexer Redis;
		private readonly Accessors _accessors;
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
			_accessors = new Accessors(configuration);
			Redis = ConnectionMultiplexer.Connect(_accessors.RedisConnectionString);
		}

        public static IConfiguration Configuration { get; private set; }

        public void ConfigureServices(IServiceCollection services)
        {
			Environment.GetEnvironmentVariable("JWT_KEY");
			Environment.SetEnvironmentVariable("AWS_ACCESS_KEY_ID", _accessors.AWSAccess.AccessKey);
			Environment.SetEnvironmentVariable("AWS_SECRET_ACCESS_KEY", _accessors.AWSAccess.SecretKey);
			Environment.SetEnvironmentVariable("AWS_REGION", _accessors.AWSAccess.Region);
			var mongoDbContext = new MongoDbContext(_accessors.ConnectionString, "Accounts");

			services.AddIdentity<AccountUser, AccountRole>()
				.AddMongoDbStores<AccountUser, AccountRole, string>(mongoDbContext)
				.AddDefaultTokenProviders();
			RegionEndpoint regionEndpoint = RegionEndpoint.EUWest2;
			IAmazonCognitoIdentityProvider amazonCognitoIdentityProvider = new AmazonCognitoIdentityProviderClient(_accessors.AWSAccess.AccessKey,_accessors.AWSAccess.SecretKey,regionEndpoint);
		
			CognitoUserPool userPool = new CognitoUserPool(_accessors.AWSPool.PoolID, _accessors.AWSPool.AppClientID,amazonCognitoIdentityProvider,_accessors.AWSPool.AppClientSecret);
			services.AddSingleton<IAmazonCognitoIdentityProvider>(amazonCognitoIdentityProvider);
			services.AddSingleton<CognitoUserPool>(userPool);

			services.AddDefaultAWSOptions(Configuration.GetAWSOptions());

			services.AddAuthorization(
			auth => {
				auth.DefaultPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme‌​)
					.RequireAuthenticatedUser()
					.Build();
				auth.AddPolicy("TrialUsers", p => p.Requirements.Add(new GroupAuthorisationRequirement(AuthTypes.TrialUsers.ToString())));
				auth.AddPolicy("BasicUsers", p=> p.Requirements.Add(new GroupAuthorisationRequirement(AuthTypes.BasicUsers.ToString())));
				auth.AddPolicy("PremiumUsers", p=> p.Requirements.Add(new GroupAuthorisationRequirement(AuthTypes.PremiumUsers.ToString())));
				auth.AddPolicy("EnterpriseUsers", p=> p.Requirements.Add(new GroupAuthorisationRequirement(AuthTypes.EnterpriseUsers.ToString())));
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
				o.Authority =_accessors.AWSPool.AuthUrl;
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

			services.AddHttpContextAccessor();

			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
			services.AddSingleton<IAuthAccessHandler>(new AuthAccessHandler(_accessors.AWSPool.AppClientSecret));
			services.AddScoped<IAuthHandler, AuthHandler>();
			services.AddScoped<IAuthorizationHandler, AuthClientHandler>();

			services.AddHangfire(options =>
			{
				options.UseFilter(new ProlongExpirationTimeAttribute());
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
				options.UseRedisStorage(Redis, new Hangfire.Redis.RedisStorageOptions
				{
					Prefix = "Timeline",
					SucceededListSize = 100000,
					DeletedListSize = 10000
				});
				options.UseSerializerSettings(new Newtonsoft.Json.JsonSerializerSettings(){ 
					ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
				});
			});

			GlobalConfiguration.Configuration.UseActivator(new WorkerActivator(services.BuildServiceProvider(false)));

			//GlobalConfiguration.Configuration.UseMongoStorage(_accessors.ConnectionString, _accessors.SchedulerDatabase, new MongoStorageOptions()
			//{
			//	Prefix = "Timeline",
			//	CheckConnection = false,
			//	MigrationOptions = new MongoMigrationOptions(MongoMigrationStrategy.Migrate)
			//	{
			//		Strategy = MongoMigrationStrategy.Migrate,
			//		BackupStrategy = MongoBackupStrategy.Collections
			//	},
			//	QueuePollInterval = TimeSpan.Zero,
			//});

			GlobalJobFilters.Filters.Add(new ProlongExpirationTimeAttribute());


			services.AddHangFrameworkServices(_accessors);
			services.AddLogics();
			services.AddContexts();
			services.AddHandlers();
			services.AddRepositories(_accessors);

			var runTime = DateBuilder.EvenMinuteDate(DateTime.UtcNow);
			var startTime = DateBuilder.NextGivenSecondDate(null, 5);

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
				ServerName = string.Format("{0}.{1}", Environment.MachineName, Guid.NewGuid().ToString())
			};

			app.UseHangfireDashboard();
			app.UseHangfireServer(jobServerOptions);
			app.UseHttpsRedirection();
			app.UseStaticFiles();	
			app.UseDefaultFiles();
			app.UseCookiePolicy();
			app.UseAuthentication();
			app.UseMvc();
		}
    }
}
