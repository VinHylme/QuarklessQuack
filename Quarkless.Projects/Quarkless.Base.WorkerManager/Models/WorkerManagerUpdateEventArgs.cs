using System;
using Quarkless.Base.WorkerManager.Models.Interfaces;

namespace Quarkless.Base.WorkerManager.Models
{
	public class WorkerManagerUpdateEventArgs : EventArgs
	{
		public int TotalWorkerCount { get; set; }
		public IWorkerManager Instance { get; set; }
		public DateTime UpdateTime { get; set; }
	}
}