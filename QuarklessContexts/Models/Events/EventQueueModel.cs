using System;

namespace QuarklessContexts.Models.Events
{
	public class EventQueueModel
	{
		public DateTime ExecutionTime { get; set; }
		public int ID { get; set; }
		public string Name { get; set; }
		public string FunctionRequest { get; set; }
		public string FunctionBody { get; set;}
	}
}
