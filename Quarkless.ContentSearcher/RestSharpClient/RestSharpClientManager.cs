using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QuarklessContexts.Models.Timeline;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace QuarklessLogic.RestSharpClient
{
	public class RestSharpClientManager : IRestSharpClientManager
	{
		private RestClient RestClient { get; set; }
		private int Attempt {get; set; } = 0;
		public RestSharpClientManager()
		{
			RestClient = new RestClient();

		}

		public void SetBaseUrl(string url)
		{
			RestClient.BaseUrl = new Uri(url);
		}
		public void AddAuthentication(string k)
		{
			RestClient.AddDefaultHeader("Authorization","Bearer {0}");
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
		private string RefreshLogin(string token, string username)
		{
			var request = new RestRequest(Method.POST);
			SetBaseUrl("http://localhost:51518/api/Auth/refreshState");
			var jsonBody = new
			{
				refreshToken = token.Replace("Bearer ",""),
				Username = username
			};
			request.AddJsonBody(JsonConvert.SerializeObject(jsonBody));
			var response = RestClient.Execute(request);
			return JObject.Parse(response.Content)["idToken"].ToString();
		}
		public IRestResponse PostRequest(string url, string resource, string jsonBody, UserStore userStore = null, 
			IEnumerable<Parameter> parameters = null, IEnumerable<HttpHeader> headers=null)
		{
			try
			{
				var request = string.IsNullOrEmpty(resource) ? new RestRequest(Method.POST): new RestRequest(resource, Method.POST);
				if(jsonBody!=null)
					request.AddJsonBody(jsonBody);

				RestClient.BaseUrl = new Uri(url);

				if (parameters!=null && parameters.Count() > 0)
				{
					foreach (var param_ in parameters)
					{
						request.AddParameter(param_);
					}
				}
				if(headers!=null && headers.Count() > 0)
				{
					foreach(var header in headers)
						request.AddHeader(header.Name,header.Value);
				}
				var response = RestClient.Execute(request);

				if(response.StatusCode == HttpStatusCode.Unauthorized && userStore!=null)
				{
					var refreshToken = RefreshLogin(headers.Where(_=>_.Name == "Authorization").Single().Value,userStore.AccessToken);
					headers.Where(_=>_.Name=="Authorization").Select(a=>a.Value = $"Bearer {refreshToken}");
					if(!string.IsNullOrEmpty(refreshToken) && Attempt <= 2) { 
						Attempt++;
						PostRequest(url,resource,jsonBody,userStore,parameters,headers);
					}
				}
				Attempt = 0;
				return response;
			}
			catch (Exception ee)
			{
				return null;
			}
		}
		public IRestResponse GetRequest(string resource, IEnumerable<Parameter> parameters=null, IEnumerable<HttpHeader> headers=null)
		{
			try
			{
				var request = new RestRequest(resource,Method.GET);
				if(parameters!=null && parameters.Count() > 0)
				{
					foreach(var param_ in parameters)
						request.AddParameter(param_);
				}
				if (headers != null && headers.Count() > 0)
				{
					foreach (var header in headers)
						request.AddHeader(header.Name, header.Value);
				}
				var response = RestClient.Execute(request);
				return response;
			}
			catch (Exception ee)
			{
				return null;
			}
		}
	}
}
