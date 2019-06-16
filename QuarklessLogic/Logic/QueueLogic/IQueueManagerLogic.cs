using QuarklessContexts.Models.Events;

namespace QuarklessLogic.Logic.QueueLogic
{
	public interface IQueueManagerLogic
	{
		bool Enqueue(EventQueueModel request);
		void Flush();
	}
}
