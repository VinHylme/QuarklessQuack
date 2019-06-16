using QuarklessContexts.Models.Events;
using QuarklessRepositories.QueueRepository;
using System;
using System.Collections.Generic;

namespace QuarklessLogic.Logic.QueueLogic
{
	public class QueueManagerLogic : IQueueManagerLogic
	{
		private readonly Queue<EventQueueModel> _requestHistory;
		private readonly IQueueManagerRepository _queueManagerRepository;
		public QueueManagerLogic(IQueueManagerRepository queueManagerRepository)
		{
			_queueManagerRepository = queueManagerRepository;

			_requestHistory = new Queue<EventQueueModel>();
		}

		public bool Enqueue(EventQueueModel request)
		{
			try
			{
				_requestHistory.Enqueue(request);
				return true;
			}
			catch (Exception ex)
			{

				return false;
			}
		}
		public void Flush()
		{
			while (_requestHistory.Count > 0)
			{
				var request = _requestHistory.Dequeue();
				try
				{
					var results = _queueManagerRepository.AddToQueue(request);
					if (!results)
						_requestHistory.Enqueue(request);
				}
				catch (Exception ex)
				{
					_requestHistory.Enqueue(request);
				}
			}
		}

	}

}
