using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using QuarklessContexts.Extensions;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Quarkless.Common.Clients;
using QuarklessContexts.Models.SecurityLayerModels;

namespace Quarkless.Services.Heartbeat
{
	class Entrance
	{
		private const string CLIENT_SECTION = "Client";
		private const string SERVER_IP = "security.quark";
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
					ServiceTypes.AddRepositories
				}
			});
			return services;
		}
		private static Task WithExceptionLogAsync(Func<Task> actionAsync)
		{
			try
			{
				return actionAsync();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"{ex.Message}");
				throw;
			}
		}
		
		static async Task Main(string[] args)
		{
			IServiceCollection services = new ServiceCollection();

			services.AddSingleton<IHeartbeatService, HeartbeatService>();
			//services.AddSingleton<IMetadataExtract, MetadataExtract>();

			services.Append(InitialiseClientServices());

			var buildService = services.BuildServiceProvider();

			await WithExceptionLogAsync(async () => await buildService.GetService<IHeartbeatService>().Start());
		}
	}
}
