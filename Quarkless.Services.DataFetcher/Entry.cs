using Microsoft.Extensions.Configuration;
using QuarklessContexts.Models.SecurityLayerModels;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Quarkless.Common.Clients;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models.ServicesModels.FetcherModels;
using QuarklessLogic.Handlers.EventHandlers;
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

		//Need to get a list of all current available topics for instagram
		//All should be extracted by specific type of worker accounts (not currently assigned to customer accounts)
		//Create fetch function for Posts Captions, Hashtags and comments (possibly account details)
		//If fetching instagram posts you have access to the captions, preview comments and possibly hashtags
		//Hashtags are also available on the first comment of a post too
		//Make sure data is clean and not duplicated
		//Captions and Comments extracted should not have any @mentions, #hashtags, advertising or non word symbols (expect emojis)
		//Store in database
		//These should be generic for that particular category,
		//If customer wants to create a clothing topic it should be generic to that topic
		static async Task Main(string[] args)
		{
			var environmentVariables = Environment.GetEnvironmentVariables();
			
			var accountId = environmentVariables["WORKER_OWNER"].ToString();
			int.TryParse(environmentVariables["WORKER_TYPE_CODE"].ToString(), out var workerType);
			int.TryParse(environmentVariables["BATCH_SIZE"].ToString(), out var batchSize);
			int.TryParse(environmentVariables["MEDIA_FETCH_AMOUNT"].ToString(), out var mediaFetchAmount);
			int.TryParse(environmentVariables["COMMENT_FETCH_AMOUNT"].ToString(), out var commentFetchAmount);
			double.TryParse(environmentVariables["HASHTAG_MEDIA_INTERVAL"].ToString(), out var intervalBetweenHashtagAndMediaSearch);
			bool.TryParse(environmentVariables["INIT"].ToString(), out var buildInitialTopics);
			if (string.IsNullOrEmpty(accountId))
				return;

			if (workerType == 0 || mediaFetchAmount == 0 || commentFetchAmount == 0 ||
			    intervalBetweenHashtagAndMediaSearch <= 0.0 || batchSize == 0)
				return;

			IServiceCollection services = new ServiceCollection();
			services.AddSingleton<IFetcher, Fetcher>();
			services.AddTransient<IFetchResolver, FetchResolver>();
			services.AddTransient<IEventSubscriberSync<MetaDataMediaRefresh>, FetchResolver>();
			services.AddTransient<IEventSubscriberSync<MetaDataCommentRefresh>, FetchResolver>();
			services.Append(InitialiseClientServices());
			var buildService = services.BuildServiceProvider();

			var settings = new Settings
			{
				AccountId = accountId,
				WorkerAccountType = workerType,
				BatchSize = batchSize,
				CommentFetchAmount = commentFetchAmount,
				MediaFetchAmount = mediaFetchAmount,
				IntervalWaitBetweenHashtagsAndMediaSearch = intervalBetweenHashtagAndMediaSearch,
				BuildInitialTopics = buildInitialTopics
			};

			await buildService.GetService<IFetchResolver>().StartService();
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
