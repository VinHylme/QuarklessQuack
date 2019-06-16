using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCore.Identity.MongoDbCore.Extensions;
using AspNetCore.Identity.MongoDbCore.Infrastructure;
using Auth_.User;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDbGenericRepository;

namespace Auth_
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
			var connections = Configuration["ConnectionStrings:MongoClientStrings"];
			var mongoDbContext = new MongoDbContext(connections, "Accounts");

			services.AddIdentity<UserApplication, RoleApplication>()
				.AddMongoDbStores<UserApplication, RoleApplication, Guid>(mongoDbContext)
				.AddDefaultTokenProviders();
			
			var mongoDbIdentityConfiguration = new MongoDbIdentityConfiguration
			{
				MongoDbSettings = new MongoDbSettings
				{
					ConnectionString =connections,
					DatabaseName = "Accounts"
				},
				IdentityOptionsAction = options =>
				{
					options.Password.RequireDigit = false;
					options.Password.RequiredLength = 8;
					options.Password.RequireNonAlphanumeric = false;
					options.Password.RequireUppercase = false;
					options.Password.RequireLowercase = false;

					// Lockout settings
					options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
					options.Lockout.MaxFailedAccessAttempts = 10;

					// ApplicationUser settings
					options.User.RequireUniqueEmail = true;
					options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789@.-_";
				}
			};

		//	services.ConfigureMongoDbIdentity<UserApplication, RoleApplication, Guid>(mongoDbIdentityConfiguration);

		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}
			app.UseHttpsRedirection();
			app.UseStaticFiles();
			app.UseDefaultFiles();
			app.UseCookiePolicy();
			app.UseAuthentication();
			app.UseMvc();
		}
	}
}
