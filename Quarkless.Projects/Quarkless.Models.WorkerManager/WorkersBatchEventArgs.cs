using System;
using Quarkless.Models.WorkerManager.Interfaces;

namespace Quarkless.Models.WorkerManager
{
	public class WorkersBatchEventArgs : EventArgs
	{
		public IWorkers Workers { get; set; }
		public DateTime Date { get; set; }
	}
}