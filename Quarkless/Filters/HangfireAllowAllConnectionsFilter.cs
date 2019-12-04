using Hangfire.Dashboard;

namespace Quarkless.Filters
{
	public class HangfireAllowAllConnectionsFilter : IDashboardAuthorizationFilter
	{
		public bool Authorize(DashboardContext context)
		{
			// Allow outside. You need an authentication scenario for this part.
			// DON'T GO PRODUCTION WITH THIS LINES.
			return true;
		}
	}
}
