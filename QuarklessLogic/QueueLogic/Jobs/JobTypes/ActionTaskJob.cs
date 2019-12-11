using QuarklessLogic.QueueLogic.Jobs.JobOptions;
using QuarklessLogic.QueueLogic.Jobs.JobRunner;
using System;

namespace QuarklessLogic.QueueLogic.Jobs.JobTypes
{
	public static class ActionTaskJobExtension
	{
		public static void QueueActionTaskJob(this IJobRunner jobRunner, Action<ActionTaskOptions> configureJob)
		{
			jobRunner.Queue<ActionTaskJob, ActionTaskOptions>(configureJob);
		}
	}

	class ActionTaskJob : IJob<ActionTaskOptions>
	{
		public void Perform(ActionTaskOptions jobOptions)
		{
			var results = jobOptions.ActionExecute.DynamicInvoke(jobOptions.Parameters);
			var prop = results.GetType().GetProperty("Succeeded");
			if (prop == null) throw new Exception("does not exist");
			var didPass = (bool) results.GetType().GetProperty("Succeeded")?.GetValue(results);
			if(!didPass)
				throw new Exception("failed");
			throw new Exception("does not exist");
		}
	}
}
