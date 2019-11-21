using Microsoft.Extensions.DependencyInjection;
using Quarkless.Common.Clients.Extensions;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models.SecurityLayerModels;

namespace Quarkless.Common.Clients.Configs
{
	internal static class Config
	{
		internal static ServiceCollection BuildServices(EnvironmentsAccess access, params ServiceTypes[] include)
		{
			if (access == null) return null;
			var services = new ServiceCollection();
			foreach (var service in include)
			{
				if (!service.GetDescription().ObjectIncludedInClass(typeof(ConfigureServices))) continue;
				if (service.GetDescription().MethodParamLength() == 1)
					service.GetDescription().InvokeConfigureMethod(services);
				else
					service.GetDescription().InvokeConfigureMethod(services, access);
			}

			return services;
		}

	}
}
