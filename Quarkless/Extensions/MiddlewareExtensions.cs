using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Quarkless.Filters;
using System;
using Quarkless.Models.Timeline.Interfaces.TaskScheduler;

namespace Quarkless.Extensions
{
	public static class HangFireExtensions
	{
		public static IApplicationBuilder ClearFailedHangfireJobs(this IApplicationBuilder app)
		{
			if (app == null)
			{
				throw new ArgumentNullException(nameof(app));
			}

			app.ApplicationServices.GetService<ITaskService>().DeleteAllFailedJobs();
			return app;
		}
	}

	public static class MiddlewareExtensions
	{
		public static IApplicationBuilder UseSecurityMiddleware(this IApplicationBuilder app, SecurityHeadersBuilder builder)
		{
			if (app == null)
			{
				throw new ArgumentNullException(nameof(app));
			}
			
			var policy = builder.Build();

			var throttlingSettings = app.ApplicationServices
				.GetService<Microsoft.Extensions.Options.IOptions<MaxConcurrentRequestsOptions>>();
			if (!throttlingSettings?.Value.Enabled ?? false)
				return app;
			
			return app.UseMiddleware<RequestResponseLoggingMiddleware>(policy);
		}
	}
}
