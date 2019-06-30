using System.Collections.Generic;
using System.Net;
using QuarklessContexts.Models.Timeline;
using RestSharp;

namespace QuarklessLogic.RestSharpClient
{
	public interface IRestSharpClientManager
	{
		void AddCookie(Cookie cookie);
		void AddCookies(IEnumerable<Cookie> cookies);
		IRestResponse GetRequest(string resource, IEnumerable<Parameter> parameters = null, IEnumerable<HttpHeader> headers = null);
		IRestResponse PostRequest(string url, string resource, string jsonBody, UserStoreDetails userStore, IEnumerable<Parameter> parameters = null, IEnumerable<HttpHeader> headers = null);
		void SetBaseUrl(string url);
	}
}