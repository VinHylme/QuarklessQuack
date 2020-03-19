using Microsoft.Extensions.DependencyInjection;
using Quarkless.Models.Shared.Extensions;
using Quarkless.Run.Services.Pipeline.Logic;
using Quarkless.Run.Services.Pipeline.Models.Interfaces;

namespace Quarkless.Run.Services.Pipeline.Models.Configuration
{
	public static class ConfigureServices
	{
		public static void IncludeServices(this IServiceCollection serviceCollection)
		{
			var environments = new Config().Environments;
			serviceCollection.AddSingleton<IAccountCreatedTransfer, AccountCreatedTransfer>();
		}
	}
}
