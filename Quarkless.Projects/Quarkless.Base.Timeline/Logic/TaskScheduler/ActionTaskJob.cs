using System;
using Quarkless.Base.Timeline.Models.Interfaces.TaskScheduler;
using Quarkless.Base.Timeline.Models.TaskScheduler;

namespace Quarkless.Base.Timeline.Logic.TaskScheduler
{
	public class ActionTaskJob : IJob<ActionTaskOptions>
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
