using System;
using Hangfire;

namespace Quarkless.Run.Services.Automation
{
	public class WorkerActivator : JobActivator
	{
		private readonly IServiceProvider _serviceProvider;

		public WorkerActivator(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}
		public override object ActivateJob(Type jobType) => _serviceProvider.GetService(jobType);
	}
}