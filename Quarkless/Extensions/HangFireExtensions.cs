using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Quarkless.Models.Timeline.Interfaces.TaskScheduler;

namespace Quarkless.Extensions
{
	public static class HangFireExtensions
	{
		/// <summary>
		/// Deletes all failed jobs and reschedules any tasks which may be due
		/// </summary>
		/// <param name="app"></param>
		/// <returns></returns>
		public static IApplicationBuilder ResetHangfireJobs(this IApplicationBuilder app)
		{
			if (app == null)
			{
				throw new ArgumentNullException(nameof(app));
			}

			app.ApplicationServices.GetService<ITaskService>().ResetJobs();
			return app;
		}
	}
}