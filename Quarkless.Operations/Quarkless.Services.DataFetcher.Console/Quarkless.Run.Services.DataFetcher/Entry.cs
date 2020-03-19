using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Quarkless.Base.Auth.Common.Models.Interfaces;
using Quarkless.Base.AuthDetails.Repository;
using Quarkless.Base.InstagramAccounts.Models.Interfaces;
using Quarkless.Base.InstagramClient.Models.Interfaces;
using Quarkless.Base.WorkerManager.Logic;
using Quarkless.Base.WorkerManager.Models.Interfaces;
using Quarkless.Run.Services.DataFetcher.Extensions;
using Quarkless.Run.Services.DataFetcher.Models.Interfaces;

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
					s.GetService<IInstagramAccountLogic>(), "lemonkaces", "5d364dbaa2b9a40f649069a6"));
			var buildService = services.BuildServiceProvider();
			using var scope = buildService.CreateScope();

			// var de = scope.ServiceProvider.GetService<IAccountRepository>();
			var fetcherService = scope.ServiceProvider.GetService<IFetchResolver>();
			await fetcherService.StartService();
		}
	}
}
