﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QuarklessContexts.Models.Timeline;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace QuarklessLogic.Handlers.RestSharpClient
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

		public IRestResponse PostRequest(string url, string resource, string jsonBody, UserStoreDetails userStore = null, 
			IEnumerable<Parameter> parameters = null, IEnumerable<HttpHeader> headers=null,string username = null, string password = null)
		{
			try
			{
				var request = string.IsNullOrEmpty(resource) ? new RestRequest(Method.POST): new RestRequest(resource, Method.POST);
				if(jsonBody!=null)
					request.AddJsonBody(jsonBody);

				RestClient.BaseUrl = new Uri(url);
				RestClient.Authenticator = new HttpBasicAuthenticator(username,password);
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
					return null;
				}
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
