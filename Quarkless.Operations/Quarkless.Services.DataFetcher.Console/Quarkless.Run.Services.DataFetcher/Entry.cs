using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quarkless.Models.Common.Extensions;
using Quarkless.Models.Services.DataFetcher.Interfaces;

namespace Quarkless.Run.Services.DataFetcher
{
	/// <summary>
	/// Purpose of this project is to extract and store data for each type of topic category available for instagram
	/// </summary>
	internal class Entry
	{
		#region Declerations
		private const string CLIENT_SECTION = "Client";
		#endregion

		static async Task Main(string[] args)
		{
			IServiceCollection services = new ServiceCollection();
			services.AddTransient<IFetchResolver, FetchResolver>();
			//services.Append(InitialiseClientServices());

			var buildService = services.BuildServiceProvider();

			using (var scope = buildService.CreateScope())
			{
				var fetcherService = scope.ServiceProvider.GetService<IFetchResolver>();
				await fetcherService.StartService();
			}
		}
	}
}
