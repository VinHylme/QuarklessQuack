using System;
using System.Collections.Generic;
using System.Text;

namespace Quarkless.Worker.Models
{
	public class ItemResults
	{
		public string Topic { get; set; }
		public List<object> Item { get; set; }
	}
}
