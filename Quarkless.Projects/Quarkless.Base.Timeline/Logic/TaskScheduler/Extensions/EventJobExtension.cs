using System;
using Quarkless.Base.Timeline.Models.Interfaces.TaskScheduler;
using Quarkless.Base.Timeline.Models.TaskScheduler;

namespace Quarkless.Base.Timeline.Logic.TaskScheduler.Extensions
{
	public static class EventJobExtension
	{
		public static string AddJob(this IJobRunner jobRunner, Action<EventActionOptions> configureJob)
		{
			return jobRunner.Queue<EventActionJob, EventActionOptions>(configureJob);
		}
	}
}