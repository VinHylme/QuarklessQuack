using System;
using Quarkless.Base.WorkerManager.Models.Interfaces;

namespace Quarkless.Base.WorkerManager.Models
{
	public class WorkersBatchEventArgs : EventArgs
	{
		public IWorkers Workers { get; set; }
		public DateTime Date { get; set; }
	}
}