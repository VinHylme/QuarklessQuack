using System;
using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;

namespace Quarkless.Models.Timeline.TaskScheduler
{
	public class ProlongExpirationTimeAttribute : JobFilterAttribute, IApplyStateFilter
	{
		public void OnStateApplied(ApplyStateContext filterContext, IWriteOnlyTransaction transaction)
		{
			filterContext.JobExpirationTimeout = TimeSpan.FromHours(24);
		}

		public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
		{
			context.JobExpirationTimeout = TimeSpan.FromHours(24);
		}
	}
}
