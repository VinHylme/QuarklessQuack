using Hangfire.Common;
using System;
using Hangfire.States;
using Hangfire.Storage;

namespace Quarkless.Base.Timeline.Models.TaskScheduler
{
	public class ProlongExpirationTimeAttribute : JobFilterAttribute, IApplyStateFilter
	{
		public int ExpiryInHours { get; set; }
		public void OnStateApplied(ApplyStateContext filterContext, IWriteOnlyTransaction transaction)
		{
			filterContext.JobExpirationTimeout = TimeSpan.FromHours(ExpiryInHours);
		}

		public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
		{
			context.JobExpirationTimeout = TimeSpan.FromHours(ExpiryInHours);
		}
	}
}
