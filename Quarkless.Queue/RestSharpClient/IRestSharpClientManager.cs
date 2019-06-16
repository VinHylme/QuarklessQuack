using System.Collections.Generic;
using System.Net;
using Quarkless.Queue.Jobs.JobOptions;
using RestSharp;

namespace Quarkless.Queue.RestSharpClient
{
	public interface IRestSharpClientManager
	{
		void AddCookie(Cookie cookie);
		void AddCookies(IEnumerable<Cookie> cookies);
		IRestResponse GetRequest(string resource, IEnumerable<Parameter> parameters = null, IEnumerable<HttpHeader> headers = null);
		IRestResponse PostRequest(string url, string resource, string jsonBody, UserStore userStore, IEnumerable<Parameter> parameters = null, IEnumerable<HttpHeader> headers = null);
		void SetBaseUrl(string url);
	}
}