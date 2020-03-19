using System;
using Quarkless.Base.Timeline.Models.Interfaces.TaskScheduler;
using Quarkless.Base.Timeline.Models.TaskScheduler;

namespace Quarkless.Base.Timeline.Logic.TaskScheduler.Extensions
{
	public static class ActionTaskJobExtension
	{
		public static void QueueActionTaskJob(this IJobRunner jobRunner, Action<ActionTaskOptions> configureJob)
		{
			jobRunner.Queue<ActionTaskJob, ActionTaskOptions>(configureJob);
		}
	}
}