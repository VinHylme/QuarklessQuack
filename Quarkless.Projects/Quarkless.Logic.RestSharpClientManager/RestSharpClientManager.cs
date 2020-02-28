using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Quarkless.Models.Proxy;
using Quarkless.Models.RestSharpClientManager;
using Quarkless.Models.RestSharpClientManager.Interfaces;
using RestSharp;
using RestSharp.Authenticators;

namespace Quarkless.Logic.RestSharpClientManager
{
	public class RestSharpClientManager : IRestSharpClientManager
	{
		private RestClient RestClient { get; set; }
		private int Attempt { get; set; } = 0;
		public RestSharpClientManager()
		{
			RestClient = new RestClient { CookieContainer = new CookieContainer() };
		}
		public void SetBaseUrl(string url)
		{
			RestClient.BaseUrl = new Uri(url);
		}
		public void AddAuthentication(string k)
		{
			RestClient.AddDefaultHeader("Authorization", "Bearer {0}");
		}
		public void AddCookie(Cookie cookie)
		{
			RestClient.CookieContainer.Add(cookie);
		}
		public void AddCookies(IEnumerable<Cookie> cookies)
		{
			foreach (var cookie in cookies)
				RestClient.CookieContainer.Add(cookie);
		}
		public void AddProxy(ProxyModel proxy)
		{
			if (proxy == null) return;
			var restClientProxy = new WebProxy(new Uri("http://" + proxy.HostAddress + ":" + proxy.Port + "/"));
			if (proxy.NeedServerAuth)
			{
				restClientProxy.Credentials = new NetworkCredential
				{
					UserName = proxy.Username,
					Password = proxy.Password
				};
			}
			else
			{
				restClientProxy.UseDefaultCredentials = true;
			}
			RestClient.Proxy = restClientProxy;
		}

		public IRestResponse PostRequest(string url, string resource, string jsonBody, UserStore userStore = null,
			IEnumerable<Parameter> parameters = null, IEnumerable<HttpHeader> headers = null,
			string username = null, string password = null)
		{
			try
			{
				var request = string.IsNullOrEmpty(resource) ? new RestRequest(Method.POST) : new RestRequest(resource, Method.POST);
				if (jsonBody != null)
					request.AddJsonBody(jsonBody);

				RestClient.BaseUrl = new Uri(url);
				if (!string.IsNullOrEmpty(username))
				{
					RestClient.Authenticator = new HttpBasicAuthenticator(username, password);
				}

				if (userStore?.OAccessToken != null)
					RestClient.Authenticator = new JwtAuthenticator(userStore.OAccessToken.Replace("Bearer ", ""));
				if (parameters != null && parameters.Any())
				{
					foreach (var param_ in parameters)
					{
						request.AddParameter(param_);
					}
				}
				if (headers != null && headers.Any())
				{
					foreach (var header in headers)
						request.AddHeader(header.Name, header.Value);
				}
				var response = RestClient.Execute(request);

				switch (response.StatusCode)
				{
					case HttpStatusCode.Unauthorized when userStore != null:
						return null;
					case HttpStatusCode.NotFound:
					case HttpStatusCode.InternalServerError:
					{
						break;
					}
				}

				return response;
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
				return null;
			}
		}
		public IRestResponse GetRequest(string url, string resource, IEnumerable<Parameter> parameters = null, IEnumerable<HttpHeader> headers = null)
		{
			try
			{
				var request = string.IsNullOrEmpty(resource) ? new RestRequest(Method.GET) : new RestRequest(resource, Method.GET);
				RestClient.BaseUrl = new Uri(url);
				
				if (parameters != null && parameters.Any())
				{
					foreach (var param_ in parameters)
						request.AddParameter(param_);
				}
				if (headers != null && headers.Any())
				{
					foreach (var header in headers)
						request.AddHeader(header.Name, header.Value);
				}
				var response = RestClient.Execute(request);
				if (response.StatusCode == HttpStatusCode.NotFound && response.StatusCode == HttpStatusCode.InternalServerError)
				{

				}
				return response;
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
				return null;
			}
		}
	}
}