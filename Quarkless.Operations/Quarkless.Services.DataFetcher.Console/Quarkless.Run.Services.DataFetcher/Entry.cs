using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using InstagramApiSharp;
using Microsoft.Extensions.DependencyInjection;
using Quarkless.Geolocation;
using Quarkless.Logic.Proxy;
using Quarkless.Logic.WorkerManager;
using Quarkless.Models.Auth.Interfaces;
using Quarkless.Models.ContentSearch.Interfaces;
using Quarkless.Models.HashtagGenerator.Interfaces;
using Quarkless.Models.InstagramAccounts.Interfaces;
using Quarkless.Models.InstagramClient.Interfaces;
using Quarkless.Models.Proxy;
using Quarkless.Models.Proxy.Interfaces;
using Quarkless.Models.ResponseResolver.Interfaces;
using Quarkless.Models.Services.DataFetcher.Interfaces;
using Quarkless.Models.Shared.Extensions;
using Quarkless.Models.Topic;
using Quarkless.Models.Topic.Interfaces;
using Quarkless.Models.WorkerManager.Interfaces;
using Quarkless.Repository.Auth;
using Quarkless.Repository.ContentSearch;
using Quarkless.Run.Services.DataFetcher.Extensions;
using Quarkless.Vision;

namespace Quarkless.Run.Services.DataFetcher
{
	/// <summary>
	/// Purpose of this project is to extract and store data for each type of topic category available for instagram
	/// </summary>
	internal class Entry
	{
		static async Task Main(string[] args)
		{
			IServiceCollection services = new ServiceCollection();
			services.IncludeServices();
			services.AddScoped<IAccountRepository, AccountRepository>();
			services.AddTransient<IFetchResolver, FetchResolver>();
			
			services.AddScoped<IWorker, Worker>(s =>
				new Worker(s.GetService<IApiClientContext>(),
					s.GetService<IInstagramAccountLogic>(), "lemonkaces", "5e397ac4a1da338090583a4f"));

			var buildService = services.BuildServiceProvider();
			using var scope = buildService.CreateScope();
			var w = scope.ServiceProvider.GetService<IWorker>();
			
			var r = await scope.ServiceProvider.GetService<IResponseResolver>()
				.WithClient(w.Client)
				.WithAttempts(1)
				.WithResolverAsync(()=> w.Client.Comment.CommentMediaAsync("2018578246234013959", "wow"));

			var de = scope.ServiceProvider.GetService<IAccountRepository>();
			var fetcherService = scope.ServiceProvider.GetService<IFetchResolver>();
			await fetcherService.StartService();
		}
	}
}
