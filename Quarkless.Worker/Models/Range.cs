using System;
using System.Collections.Generic;
using System.Text;

namespace Quarkless.Worker.Models
{
	public class Ranges
	{
		public int Start { get; set; }
		public int End { get; set; }

		public Ranges(int start, int end)
		{
			this.Start = start;
			this.End = end;
		}
	}
}
