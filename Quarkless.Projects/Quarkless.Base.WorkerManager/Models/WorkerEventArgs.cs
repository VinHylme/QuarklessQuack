using System;
using Quarkless.Base.WorkerManager.Models.Interfaces;

namespace Quarkless.Base.WorkerManager.Models
{
	public class WorkerEventArgs : EventArgs
	{
		public IWorker Worker { get; set; }
		public DateTime Date { get; set; }
	}
}