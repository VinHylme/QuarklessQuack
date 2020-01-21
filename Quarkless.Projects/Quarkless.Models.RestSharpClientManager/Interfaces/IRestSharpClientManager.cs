using Quarkless.Models.Proxy;
using RestSharp;
using System.Collections.Generic;
using System.Net;

namespace Quarkless.Models.RestSharpClientManager.Interfaces
{
	public interface IRestSharpClientManager
	{
		void AddProxy(ProxyModel proxy);
		void AddCookie(Cookie cookie);
		void AddCookies(IEnumerable<Cookie> cookies);
		IRestResponse GetRequest(string url, string resource, IEnumerable<Parameter> parameters = null, IEnumerable<HttpHeader> headers = null);
		IRestResponse PostRequest(string url, string resource, string jsonBody, UserStore userStore = null,
			IEnumerable<Parameter> parameters = null, IEnumerable<HttpHeader> headers = null, string username = null, string password = null);
		void SetBaseUrl(string url);
	}
}
