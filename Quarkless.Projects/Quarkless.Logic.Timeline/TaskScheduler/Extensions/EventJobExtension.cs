using System;
using Quarkless.Models.Timeline.Interfaces.TaskScheduler;
using Quarkless.Models.Timeline.TaskScheduler;

namespace Quarkless.Logic.Timeline.TaskScheduler.Extensions
{
	public static class EventJobExtension
	{
		public static string AddJob(this IJobRunner jobRunner, Action<EventActionOptions> configureJob)
		{
			return jobRunner.Queue<EventActionJob, EventActionOptions>(configureJob);
		}
	}
}