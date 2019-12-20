using System;
using System.Collections.Generic;

namespace QuarklessLogic.Handlers.WorkerManagerService
{
	public class WorkersBatchEventArgs : EventArgs
	{
		public IEnumerable<Worker> Workers;
		public DateTime Date { get; set; }
	}
}