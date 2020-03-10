using Hangfire.Dashboard;

namespace Quarkless.Filters
{
	public class HangfireAllowOnlyAdminConnectionsFilter : IDashboardAuthorizationFilter
	{
		public HangfireAllowOnlyAdminConnectionsFilter()
		{
		}

		public bool Authorize(DashboardContext context)
		{
			return true; //for now allow any connection
		}
	}
}
