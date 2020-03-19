using System;
using Microsoft.Extensions.DependencyInjection;
using Quarkless.Base.Timeline.Models.Interfaces.TaskScheduler;

namespace Quarkless.Run.Services.Scheduler.Extensions
{
	public static class HangFireExtensions
	{
		/// <summary>
		/// Deletes all failed jobs and reschedules any tasks which may be due
		/// </summary>
		/// <param name="provider"></param>
		/// <returns></returns>
		public static IServiceProvider ResetHangfireJobs(this IServiceProvider provider)
		{
			if (provider == null)
			{
				throw new ArgumentNullException(nameof(provider));
			}

			provider.GetService<ITaskService>().ResetJobs();
			return provider;
		}
	}
}