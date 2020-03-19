using System;
using Quarkless.Base.WorkerManager.Models.Interfaces;
using Quarkless.Models.Common.Models.Carriers;

namespace Quarkless.Base.WorkerManager.Models
{
	public class WorkerFailedEventArgs : EventArgs
	{
		public IWorker Worker { get; set; }
		public DateTime Date { get; set; }
		public ErrorResponse Error { get; set; }
	}
}