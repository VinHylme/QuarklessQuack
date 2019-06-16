using Quarkless.Queue.Interfaces.Jobs;
using Quarkless.Queue.Jobs.Interfaces;
using Quarkless.Queue.Jobs.JobOptions;
using Quarkless.Queue.Jobs.JobTypes;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Quarkless.Queue.Services
{
	public class TaskService : ITaskService
	{
		private readonly IJobRunner _jobRunner;
		public TaskService(IJobRunner jobRunner)
		{
			_jobRunner = jobRunner;
		}

		public void ActionTask(Delegate @delegate, DateTimeOffset executeTime, params object[] args)
		{
			_jobRunner.QueueActionTaskJob(o =>
			{
				o.ActionExecute = @delegate;
				o.ExecutionTime = executeTime;
				o.Parameters = args;
			});
		}

		public void LongRunningTask(RestModel restModel, DateTimeOffset timeOffset)
		{
			_jobRunner.QueueLongRunningJob(op=>
			{
				op.Rest = restModel;
				op.ExecutionTime = timeOffset;
			});
		}

		public void RepeatingTask()
		{
			throw new NotImplementedException();
		}
	}
}
