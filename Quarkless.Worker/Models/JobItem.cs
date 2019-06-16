using Quarkless.Worker.Actions;
using Quarkless.Worker.TaskScheduler;
using System;
using System.Collections.Generic;
using System.Text;

namespace Quarkless.Worker.Models
{
	public enum JobStatus
	{
		NotStarted,
		InProgress,
		Finished,
		Error
	}
	public class Assigned
	{
		public object Item { get; set; }
		public string Topic { get; set; }
		public int Limit { get; set; }
		public JobStatus JobStatus { get; set; } = JobStatus.NotStarted;
	}
	public class JobItem
	{
		public ActionType AssignedWorker { get; set; }
		public Assigned AssignedItems { get; set; }
	}
}
