using System;
using System.IO;
using System.Reflection;
using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quarkless.Logic.Services.Pipeline;
using Quarkless.Models.Services.Pipeline.Interfaces;
using Quarkless.Run.Services.Pipeline.Configuration;

namespace Quarkless.Run.Services.Pipeline
{
	public class Program
	{
		static void Main(string[] args)
		{
			Autofac.ContainerBuilder containerBuilder = new ContainerBuilder();
			var d = Assembly.LoadFrom(@"C:\Users\yousef.alaw\source\repos\QuarklessQuack\Quarkless\bin\Debug\netcoreapp3.1");
			//containerBuilder.RegisterAssemblyTypes(Assembly.Load());
			containerBuilder.RegisterType<AccountCreatedTransfer>().As<IAccountCreatedTransfer>()
				.InstancePerLifetimeScope();
			var b = containerBuilder.Build();
			using var scope = b.BeginLifetimeScope();
			scope.Resolve<IAccountCreatedTransfer>().Test();
			// var service = InitialiseClientServices().BuildServiceProvider();
			// using var scope = service.CreateScope();
			// var test = scope.ServiceProvider.GetService<IAccountCreatedTransfer>();
			// test.Test();
		}

		#region Build Services
		private static IServiceCollection InitialiseClientServices()
		{
			var localServices = new ServiceCollection();
			localServices.IncludeServices();
			return localServices;
		}
		#endregion
	}
}
