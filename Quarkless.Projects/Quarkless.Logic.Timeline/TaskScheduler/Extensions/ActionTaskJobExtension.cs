using System;
using Quarkless.Models.Timeline.Interfaces.TaskScheduler;
using Quarkless.Models.Timeline.TaskScheduler;

namespace Quarkless.Logic.Timeline.TaskScheduler.Extensions
{
	public static class ActionTaskJobExtension
	{
		public static void QueueActionTaskJob(this IJobRunner jobRunner, Action<ActionTaskOptions> configureJob)
		{
			jobRunner.Queue<ActionTaskJob, ActionTaskOptions>(configureJob);
		}
	}
}