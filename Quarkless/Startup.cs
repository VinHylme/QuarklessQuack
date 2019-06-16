using System;
using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.Extensions.CognitoAuthentication;
using Hangfire;
using Hangfire.Mongo;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDbGenericRepository;
using Quarkless.Auth;
using Quarkless.Auth.AuthTypes;
using Quarkless.Auth.Manager;
using Quarkless.Common;
using QuarklessContexts.Contexts;
using QuarklessContexts.Contexts.AccountContext;
using QuarklessLogic.Handlers.ReportHandler;
using QuarklessLogic.Logic.CollectionsLogic;
using QuarklessLogic.Logic.CommentLogic;
using QuarklessLogic.Logic.DiscoverLogic;
using QuarklessLogic.Logic.InstaAccountOptionsLogic;
using QuarklessLogic.Logic.InstagramAccountLogic;
using QuarklessLogic.Logic.InstaUserLogic;
using QuarklessLogic.Logic.ProfileLogic;
using QuarklessLogic.Logic.ProxyLogic;
using QuarklessRepositories.InstagramAccountRepository;
using QuarklessRepositories.ProfileRepository;
using QuarklessRepositories.ProxyRepository;
using QuarklessRepositories.RepositoryClientManager;
using Quartz;

namespace Quarkless
{
    public class Startup
    {
        //public const string AppS3BucketKey = "AppS3Bucket";
		private readonly Accessors _accessors;
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
			_accessors = new Accessors(configuration);
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
			IAmazonCognitoIdentityProvider amazonCognitoIdentityProvider = 
				new AmazonCognitoIdentityProviderClient(_accessors.AWSAccess.AccessKey,_accessors.AWSAccess.SecretKey,regionEndpoint);
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
			services.AddSingleton<IAuthHandler, AuthHandler>();
			services.AddSingleton<IAuthorizationHandler, AuthClientHandler>();

			services.AddHangfire(options =>
			{
				options.UseMongoStorage(_accessors.ConnectionString, _accessors.ControlDatabase, new MongoStorageOptions()
				{
					CheckConnection = false,
					MigrationOptions = new MongoMigrationOptions(MongoMigrationStrategy.Migrate) { Strategy = MongoMigrationStrategy.Migrate }
				});
				options.UseSerializerSettings(new Newtonsoft.Json.JsonSerializerSettings(){ 
					ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
				});
			});
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
			app.UseHangfireDashboard();
			app.UseHangfireServer();
			app.UseHttpsRedirection();
			app.UseStaticFiles();	
			app.UseDefaultFiles();
			app.UseCookiePolicy();
			app.UseAuthentication();
			app.UseMvc();
		}
    }
}
