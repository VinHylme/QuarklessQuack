using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InstagramApiSharp;
using Microsoft.Extensions.DependencyInjection;
using Quarkless.Logic.WorkerManager;
using Quarkless.Models.ContentSearch.Interfaces;
using Quarkless.Models.HashtagGenerator.Interfaces;
using Quarkless.Models.InstagramAccounts.Interfaces;
using Quarkless.Models.InstagramClient.Interfaces;
using Quarkless.Models.ResponseResolver.Interfaces;
using Quarkless.Models.Services.DataFetcher.Interfaces;
using Quarkless.Models.Shared.Extensions;
using Quarkless.Models.Topic;
using Quarkless.Models.Topic.Interfaces;
using Quarkless.Models.WorkerManager.Interfaces;
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
			services.AddTransient<IFetchResolver, FetchResolver>();
			services.AddScoped<IWorker, Worker>(s =>
				new Worker(s.GetService<IApiClientContext>(),
					s.GetService<IInstagramAccountLogic>(), "lemonkaces", "5d364dbaa2b9a40f649069a6"));

			var buildService = services.BuildServiceProvider();
			using var scope = buildService.CreateScope();
			var worker = scope.ServiceProvider.GetService<IWorker>();
			var resolver = scope.ServiceProvider.GetService<IResponseResolver>();

			var res = await resolver
				.WithClient(worker.Client)
				.WithAttempts(1)
				.WithResolverAsync(()=>worker.Client.Feeds
					.GetRecentActivityFeedAsync(PaginationParameters.MaxPagesToLoad(1)));

			if (res.Succeeded)
			{

			}
			
			var fetcherService = scope.ServiceProvider.GetService<IFetchResolver>();
			await fetcherService.StartService();
		}
	}
}
