using System;
using Quarkless.Models.Common.Models.Carriers;
using Quarkless.Models.WorkerManager.Interfaces;

namespace Quarkless.Models.WorkerManager
{
	public class WorkerFailedEventArgs : EventArgs
	{
		public IWorker Worker { get; set; }
		public DateTime Date { get; set; }
		public ErrorResponse Error { get; set; }
	}
}