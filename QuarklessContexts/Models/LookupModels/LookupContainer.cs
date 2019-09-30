using System.Collections.Generic;

namespace QuarklessContexts.Models.LookupModels
{
	public class LookupContainer<T>
	{
		public IEnumerable<LookupModel> Lookup { get; set; }
		public T Object { get; set; }
	}
}
