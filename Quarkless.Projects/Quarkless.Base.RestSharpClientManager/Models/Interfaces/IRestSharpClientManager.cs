using System.Collections.Generic;
using System.Net;
using Quarkless.Base.Proxy.Models;
using RestSharp;

namespace Quarkless.Base.RestSharpClientManager.Models.Interfaces
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
