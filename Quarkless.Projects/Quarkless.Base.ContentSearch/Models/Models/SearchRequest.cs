using System.Collections.Generic;

namespace Quarkless.Base.ContentSearch.Models.Models
{
	public class SearchRequest
	{
		public int Limit { get; set; }
		public int Offset { get; set; }
		public IEnumerable<string> RequestData { get; set; }
		public Quarkless.Models.SearchResponse.Media ResponseData { get; set; }
	}

	public class UserSearchRequest
	{
		public string Query { get; set; }
	}
}
