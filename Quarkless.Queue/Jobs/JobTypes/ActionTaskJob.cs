using Quarkless.Queue.Jobs.JobOptions;
using QuarklessContexts.JobClass;
using System;

namespace Quarkless.Queue.Jobs.JobTypes
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
			if (prop != null) { 
				var didPass = (bool) results.GetType().GetProperty("Succeeded").GetValue(results);
				if(!didPass)
					throw new Exception("failed");
			}
			throw new Exception("does not exist");
		}
	}
}
