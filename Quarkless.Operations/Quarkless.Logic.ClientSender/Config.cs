using Microsoft.Extensions.DependencyInjection;
using Quarkless.Base.ClientSender.Extensions;
using Quarkless.Base.ClientSender.Extensions.Configs;
using Quarkless.Models.ClientSender;
using Quarkless.Models.ClientSender.Enums;
using Quarkless.Models.Common.Extensions;

namespace Quarkless.Logic.ClientSender
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