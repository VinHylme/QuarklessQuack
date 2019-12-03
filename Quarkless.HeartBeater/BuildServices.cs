using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quarkless.Common.Clients;
using Quarkless.HeartBeater.__Init__;
using Quarkless.Services.ContentBuilder.TopicBuilder;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models.SecurityLayerModels;
using ServiceReacher = Quarkless.HeartBeater.__Init__.ServiceReacher;

namespace Quarkless.HeartBeater
{
	public class BuildServices
	{
		private const string CLIENT_SECTION = "Client";
		private const string SERVER_IP = "quarkless.security";

		private IConfiguration MakeConfigurationBuilder()
		{
			return new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json").Build();
		}
		private IServiceCollection InitialiseClientServices()
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

			var services = (IServiceCollection) cIn.Send(new BuildCommandArgs()
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

		public ServiceReacher Build()
		{
			var servicesIn = new ServiceCollection();

			servicesIn.AddSingleton<IInit, Init>();
			servicesIn.AddTransient<ITopicBuilder, TopicBuilder>();
			servicesIn.AddLogging();

			var services = InitialiseClientServices();
			services.Append(servicesIn);

			return new ServiceReacher(services.BuildServiceProvider());
		}
	}
}
