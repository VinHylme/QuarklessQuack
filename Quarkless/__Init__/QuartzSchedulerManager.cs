using Quartz;
using Quartz.Impl;
using System;

namespace Quarkless.__Init__
{
	public class QuartzSchedulerManager
	{
		public static async void InitialiseQuartz(DateTimeOffset runTime, DateTimeOffset startTime, string jobName = "ConsumeActions", string triggerName = "triggerActions", string groupName = "ActionHandlers")
		{
			ISchedulerFactory schedulerFactory = new StdSchedulerFactory();
			IScheduler scheduler = await schedulerFactory.GetScheduler();

			IJobDetail job = JobBuilder.Create<QueueConsumer>()
				.WithIdentity(jobName, groupName)
				.Build();
			ITrigger trigger = TriggerBuilder.Create()
				.WithIdentity(triggerName, groupName)
				.StartAt(runTime)
				.WithCronSchedule("5 0/1 * * * ?")
				.Build();
			await scheduler.ScheduleJob(job, trigger);
			await scheduler.Start();
		}
	}
}
