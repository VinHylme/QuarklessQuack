using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;
using System;
using System.Collections.Generic;
using System.Text;

namespace Quarkless.Queue.Jobs.Filters
{
	public class ProlongExpirationTimeAttribute : JobFilterAttribute, IApplyStateFilter
	{
		public void OnStateApplied(ApplyStateContext filterContext, IWriteOnlyTransaction transaction)
		{
			filterContext.JobExpirationTimeout = TimeSpan.FromDays(1);
		}

		public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
		{
			context.JobExpirationTimeout = TimeSpan.FromDays(1);
		}
	}
}
