using System;

namespace QuarklessLogic.Handlers.WorkerManagerService
{
	public class WorkersBatchEventArgs : EventArgs
	{
		public Workers Workers { get; set; }
		public DateTime Date { get; set; }
	}

}