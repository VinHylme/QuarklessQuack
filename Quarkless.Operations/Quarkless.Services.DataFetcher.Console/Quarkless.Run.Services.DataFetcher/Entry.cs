using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Quarkless.Logic.WorkerManager;
using Quarkless.Models.Auth.Interfaces;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.InstagramAccounts.Interfaces;
using Quarkless.Models.InstagramClient.Interfaces;
using Quarkless.Models.Notification;
using Quarkless.Models.Notification.Interfaces;
using Quarkless.Models.Services.DataFetcher.Interfaces;
using Quarkless.Models.WorkerManager.Interfaces;
using Quarkless.Repository.Auth;
using Quarkless.Repository.Notification;
using Quarkless.Run.Services.DataFetcher.Extensions;

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
