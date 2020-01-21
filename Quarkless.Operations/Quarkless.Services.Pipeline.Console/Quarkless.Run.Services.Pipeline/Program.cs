using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quarkless.Models.Services.Pipeline.Interfaces;

namespace Quarkless.Run.Services.Pipeline
{
	public class Program
	{
		static void Main(string[] args)
		{
			var service = InitialiseClientServices().BuildServiceProvider();
			using (var scope = service.CreateScope())
			{
				var test = scope.ServiceProvider.GetService<IAccountCreatedTransfer>();
				test.Test();
			}
		}

		#region Build Services
		private static IServiceCollection InitialiseClientServices()
		{
			var localServices = new ServiceCollection();
			//localServices.IncludeServices();
			return localServices;
		}
		#endregion
	}
}
