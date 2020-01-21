using System.Collections.Generic;
using Quarkless.Models.SearchResponse;

namespace Quarkless.Models.ContentSearch.Models
{
	public class SearchRequest
	{
		public int Limit { get; set; }
		public int Offset { get; set; }
		public IEnumerable<string> RequestData { get; set; }
		public Media ResponseData { get; set; }
	}

	public class UserSearchRequest
	{
		public string Query { get; set; }
	}
}
