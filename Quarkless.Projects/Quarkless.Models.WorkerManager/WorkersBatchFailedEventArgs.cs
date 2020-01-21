using Quarkless.Models.Common.Models.Carriers;
using System;
using Quarkless.Models.WorkerManager.Interfaces;

namespace Quarkless.Models.WorkerManager
{
	public class WorkersBatchFailedEventArgs : EventArgs
	{
		public IWorkers Workers;
		public DateTime Date { get; set; }
		public ErrorResponse Error { get; set; }
	}
}