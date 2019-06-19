using QuarklessContexts.Models.Timeline;
using System;

namespace Quarkless.Queue.Jobs.Interfaces
{
	public interface ITaskService
	{
		void LongRunningTask(RestModel restModel, DateTimeOffset timeOffset);
		void ActionTask(Delegate @delegate, DateTimeOffset executeTime, params object[] args);
		void RepeatingTask();
	}
}
