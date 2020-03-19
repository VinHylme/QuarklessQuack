using Microsoft.Extensions.DependencyInjection;
using Quarkless.Run.Services.Pipeline.Models.Configuration;
using Quarkless.Run.Services.Pipeline.Models.Interfaces;

namespace Quarkless.Run.Services.Pipeline
{
	public class Program
	{
		static void Main(string[] args)
		{
			var localServices = new ServiceCollection();
			localServices.IncludeServices();
			using var scope = localServices.BuildServiceProvider().CreateScope();
			var instance = scope.ServiceProvider.GetService<IAccountCreatedTransfer>();
			instance.Test();
		}
	}
}
