using System.Collections.Generic;
using Quarkless.Base.Lookup.Models;

namespace Quarkless.Base.Query.Models
{
	public class LookupContainer<T>
	{
		public IEnumerable<LookupModel> Lookup { get; set; }
		public T Object { get; set; }
	}
}
