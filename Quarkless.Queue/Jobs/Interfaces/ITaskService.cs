using Quarkless.Queue.Jobs.JobOptions;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Quarkless.Queue.Jobs.Interfaces
{
	public interface ITaskService
	{
		void LongRunningTask(RestModel restModel, DateTimeOffset timeOffset);
		void ActionTask(Delegate @delegate, DateTimeOffset executeTime, params object[] args);
		void RepeatingTask();
	}
}
