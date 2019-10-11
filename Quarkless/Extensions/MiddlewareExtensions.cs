using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quarkless.Filters;
using System;

namespace Quarkless.Extensions
{
	public static class MiddlewareExtensions
	{
		public static IApplicationBuilder UseSecurityMiddleware(this IApplicationBuilder app, SecurityHeadersBuilder builder)
		{
			if (app == null)
			{
				throw new ArgumentNullException(nameof(app));
			}
			
			var policy = builder.Build();

			var throttlingSettings = app.ApplicationServices.GetService<Microsoft.Extensions.Options.IOptions<MaxConcurrentRequestsOptions>>();
			if (!throttlingSettings?.Value.Enabled ?? false)
				return app;
			
			return app.UseMiddleware<RequestResponseLoggingMiddleware>(policy);
		}

		public static IServiceCollection ConfigureRequestThrottleServices(this IServiceCollection services, IConfiguration configuration)
		{
			services.Configure<MaxConcurrentRequestsOptions>(configuration.GetSection("MaxConcurrentRequests"));
			return services;
		}
	}
}
