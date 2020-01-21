using System;
using Quarkless.Models.WorkerManager.Interfaces;

namespace Quarkless.Models.WorkerManager
{
	public class WorkerManagerUpdateEventArgs : EventArgs
	{
		public int TotalWorkerCount { get; set; }
		public IWorkerManager Instance { get; set; }
		public DateTime UpdateTime { get; set; }
	}
}