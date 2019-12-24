using System;
using QuarklessContexts.Classes.Carriers;

namespace QuarklessLogic.Handlers.WorkerManagerService
{
	public class WorkersBatchFailedEventArgs : EventArgs
	{
		public Workers Workers;
		public DateTime Date { get; set; }
		public ErrorResponse Error { get; set; }
	}
}