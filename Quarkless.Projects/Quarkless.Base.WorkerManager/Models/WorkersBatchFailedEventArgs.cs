using System;
using Quarkless.Base.WorkerManager.Models.Interfaces;
using Quarkless.Models.Common.Models.Carriers;

namespace Quarkless.Base.WorkerManager.Models
{
	public class WorkersBatchFailedEventArgs : EventArgs
	{
		public IWorkers Workers;
		public DateTime Date { get; set; }
		public ErrorResponse Error { get; set; }
	}
}