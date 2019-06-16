using QuarklessContexts.Models.Events;
using System;

namespace QuarklessRepositories.QueueRepository
{
	public class QueueManagerRepository : IQueueManagerRepository
	{
		public QueueManagerRepository()
		{

		}
		public bool AddToQueue(EventQueueModel eventQueue)
		{
			throw new NotImplementedException();
		}
	}
}
