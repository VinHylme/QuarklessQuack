using Microsoft.Extensions.Configuration;
using QuarklessContexts.Models.SecurityLayerModels;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Quarkless.Common.Clients;
using Quarkless.Services.DataFetcher.FetchResolver;
using QuarklessContexts.Extensions;
namespace Quarkless.Services.DataFetcher
{
	/// <summary>
	/// Purpose of this project is to extract and store data for each type of topic category available for instagram
	/// </summary>
	internal class Entry
	{
		#region Declerations
		private const string CLIENT_SECTION = "Client";
		private const string SERVER_IP = "security.quark";
		#endregion

		static async Task Main(string[] args)
		{
			IServiceCollection services = new ServiceCollection();
			services.AddTransient<IFetchResolver, FetchResolver.FetchResolver>();
			services.Append(InitialiseClientServices());

			var buildService = services.BuildServiceProvider();

			using (var scope = buildService.CreateScope())
			{
				var fetcherService = scope.ServiceProvider.GetService<IFetchResolver>();
				await fetcherService.StartService();
			}
		}

		#region Build Services
		private static IConfiguration MakeConfigurationBuilder()
		{
			return new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory().Split("bin")[0])
				.AddJsonFile("appsettings.json").Build();
		}
		private static IServiceCollection InitialiseClientServices()
		{
			var cIn = new ClientRequester(SERVER_IP);
			if (!cIn.TryConnect().GetAwaiter().GetResult())
				throw new Exception("Invalid Client");

			var caller = MakeConfigurationBuilder().GetSection(CLIENT_SECTION).Get<AvailableClient>();

			var validate = cIn.Send(new InitCommandArgs
			{
				Client = caller,
				CommandName = "Validate Client"
			});
			if (!(bool)validate)
				throw new Exception("Could not validated");

			var services = (IServiceCollection)cIn.Send(new BuildCommandArgs()
			{
				CommandName = "Build Services",
				ServiceTypes = new[]
				{
					ServiceTypes.AddConfigurators,
					ServiceTypes.AddContexts,
					ServiceTypes.AddHandlers,
					ServiceTypes.AddLogics,
					ServiceTypes.AddRepositories,
					ServiceTypes.AddEventServices
				}
			});
			return services;
		}
		#endregion
	}
}
