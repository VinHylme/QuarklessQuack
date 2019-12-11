using Microsoft.Extensions.Configuration;
using QuarklessContexts.Models.SecurityLayerModels;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Quarkless.Common.Clients;
using QuarklessContexts.Extensions;
using QuarklessLogic.ServicesLogic;

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

		//TODO: Need to get a list of all current available topics for instagram
		//TODO: All should be extracted by specific type of worker accounts (not currently assigned to customer accounts)
		//TODO: Create fetch function for Posts Captions, Hashtags and comments (possibly account details)
		//TODO: If fetching instagram posts you have access to the captions, preview comments and possibly hashtags
		//TODO: Hashtags are also available on the first comment of a post too
		//TODO: Make sure data is clean and not duplicated
		//TODO: Captions and Comments extracted should not have any @mentions, #hashtags, advertising or non word symbols (expect emojis)
		//TODO: Store in database
		//TODO: These should be generic for that particular category,
		//TODO: If customer wants to create a clothing topic it should be generic to that topic
		static async Task Main(string[] args)
		{
			var environmentVariables = Environment.GetEnvironmentVariables();
			var accountId = environmentVariables["WORKER_OWNER"].ToString();

			if (string.IsNullOrEmpty(accountId))
				return;

			IServiceCollection services = new ServiceCollection();
			services.AddSingleton<IFetcher, Fetcher>();
			services.Append(InitialiseClientServices());
			var buildService = services.BuildServiceProvider();

			var settings = new Settings{ AccountId = accountId };

			await buildService.GetService<IFetcher>().Begin(settings);
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
					ServiceTypes.AddRepositories
				}
			});
			return services;
		}
		#endregion
	}
}
