using QuarklessContexts.Models.Events;

namespace QuarklessRepositories.QueueRepository
{
	public interface IQueueManagerRepository
	{
		bool AddToQueue(EventQueueModel eventQueue);

	}
}