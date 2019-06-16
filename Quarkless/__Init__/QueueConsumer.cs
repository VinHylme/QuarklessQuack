using QuarklessLogic.Logic.QueueLogic;
using Quartz;
using System.Threading.Tasks;

namespace Quarkless.__Init__
{
	public class QueueConsumer : IJob
	{
		private readonly IQueueManagerLogic _queueManagerLogic;

		public QueueConsumer(IQueueManagerLogic queueManagerLogic)
		{
			_queueManagerLogic = queueManagerLogic;
		}

		Task IJob.Execute(IJobExecutionContext context)
		{
			Task task = new Task(() =>
			{
				_queueManagerLogic.Flush();
			});
			task.Start();
			return task;
		}
	}
}
