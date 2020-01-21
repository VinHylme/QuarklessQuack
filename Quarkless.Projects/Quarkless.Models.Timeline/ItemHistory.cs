using System;
using System.Collections.Generic;

namespace Quarkless.Models.Timeline
{
	public class ItemHistory
	{
		public DateTime CreatedAt { get; set; }
		public string Reason { get ; set; }
		public string StateName { get; set; }
		public IDictionary<string,string> Data = new Dictionary<string, string>();
	}
}