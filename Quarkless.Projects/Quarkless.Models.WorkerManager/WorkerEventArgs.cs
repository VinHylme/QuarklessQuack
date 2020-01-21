using System;
using Quarkless.Models.WorkerManager.Interfaces;

namespace Quarkless.Models.WorkerManager
{
	public class WorkerEventArgs : EventArgs
	{
		public IWorker Worker { get; set; }
		public DateTime Date { get; set; }
	}
}