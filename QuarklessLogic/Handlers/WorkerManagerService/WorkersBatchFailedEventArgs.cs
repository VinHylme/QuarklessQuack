using System;
using System.Collections.Generic;
using QuarklessContexts.Classes.Carriers;

namespace QuarklessLogic.Handlers.WorkerManagerService
{
	public class WorkersBatchFailedEventArgs : EventArgs
	{
		public IEnumerable<Worker> Workers;
		public DateTime Date { get; set; }
		public ErrorResponse Error { get; set; }
	}
}