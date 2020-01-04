using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MoreLinq;
using Quarkless.Common.Clients;
using Quarkless.Services.Pipeline.NewAccountsManager;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models.SecurityLayerModels;

namespace Quarkless.Services.Pipeline
{
	public class Program
	{
		private const string CLIENT_SECTION = "Client";
		private const string SERVER_IP = "security.quark";
		static async Task Main(string[] args)
		{
			var service = InitialiseClientServices().BuildServiceProvider();
			using (var scope = service.CreateScope())
			{
				var test = scope.ServiceProvider.GetService<ITest>();
				await test.TestStart();
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
			var localServices = new ServiceCollection();

			var services = cIn.Build(caller, ServiceTypes.AddConfigurators,
				ServiceTypes.AddContexts,
				ServiceTypes.AddHandlers,
				ServiceTypes.AddLogics,
				ServiceTypes.AddRepositories,
				ServiceTypes.AddEventServices);
			if (services == null)
				throw new Exception("Could not retrieve services");

			localServices.Append(services);
			localServices.AddSingleton<ITest, Test>();
			return localServices;
		}
		#endregion
	}
}
