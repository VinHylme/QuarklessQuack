using QuarklessContexts.Enums;
using RestSharp;
using System.Collections.Generic;

namespace QuarklessContexts.Models.Timeline
{
	public class RestModel
	{
		public UserStore User { get; set; }
		public string BaseUrl { get; set; }
		public string JsonBody { get; set; }
		public string ResourceAction { get; set; }
		public IEnumerable<Parameter> Parameters { get; set; }
		public RequestType RequestType { get; set; }
		public List<HttpHeader> RequestHeaders { get; set; }

		public RestModel()
		{
			RequestHeaders = new List<HttpHeader>();
		}

	}
}
