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
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Newtonsoft.Json;
using Quarkless.Extensions;
using Quarkless.Filters;
using Quarkless.Models.Auth.Interfaces;
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
			services.IncludeRepositories();
			services.IncludeAuthHandlers();
			services.IncludeRequestLogging();
			services.IncludeConfigurators();
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

			#endregion

			services.AddHangfire(options =>
			{
				options.UseRedisStorage(redis, new Hangfire.Redis.RedisStorageOptions
				{
					Db = 1,
					Prefix = REDIS_DB_NAME,
					SucceededListSize = 10000,
					DeletedListSize = 10000,
					InvisibilityTimeout = TimeSpan.FromMinutes(30),
					FetchTimeout = TimeSpan.FromMinutes(30),
					UseTransactions = true
				});
			});
			GlobalConfiguration.Configuration.UseSerializerSettings(new JsonSerializerSettings()
			{
				ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
			});
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
				app.UseDeveloperExceptionPage();
			else
				app.UseHsts();

			app.UseHttpsRedirection();
			app.UseRouting();
			//app.UseIpRateLimiting();

			app.UseCors(CORS_POLICY);
			app.UseHangfireDashboard("/configs/hang-fire", new DashboardOptions
			{
				DisplayStorageConnectionString = false,
				Authorization = new[]
				{
					new HangfireAllowOnlyAdminConnectionsFilter()
				}
			});
			
			app.UseStaticFiles();	
			app.UseDefaultFiles();
			app.UseCookiePolicy();
			app.UseAuthentication();
			app.UseAuthorization();
			app.UseEndpoints(options =>
			{
				options.MapControllers();
			});

			app.UseSecurityMiddleware(new SecurityHeadersBuilder()
				.AddDefaultSecurePolicy()
				.AddCustomHeader("X-Client-ID", "x-key-1-v"));
		}
	}
}
