using System;
using Microsoft.Extensions.DependencyInjection;

namespace Quarkless.HeartBeater.__Init__
{
	public class ServiceReacher
	{
		private readonly IServiceProvider _serviceProvider;
		public ServiceReacher(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}
		public TInstance Get<TInstance>() => _serviceProvider.GetService<TInstance>();
	}
}
