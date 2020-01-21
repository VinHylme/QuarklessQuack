using Quarkless.Models.Lookup;
using System.Collections.Generic;

namespace Quarkless.Models.Query
{
	public class LookupContainer<T>
	{
		public IEnumerable<LookupModel> Lookup { get; set; }
		public T Object { get; set; }
	}
}
