using System;
using Quarkless.Models.Timeline.Interfaces.TaskScheduler;
using Quarkless.Models.Timeline.TaskScheduler;

namespace Quarkless.Logic.Timeline.TaskScheduler.Extensions
{
	public static class LongRunningJobExtension
	{
		public static string AddScheduledJob(this IJobRunner jobRunner, Action<LongRunningJobOptions> configureJob)
		{
			return jobRunner.Queue<LongRunningJob, LongRunningJobOptions>(configureJob);
		}
	}
}
