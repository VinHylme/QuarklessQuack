using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Quarkless.Queue.Jobs.JobOptions;
using Quarkless.Queue.RestSharpClient;
using Quarkless.Services.RequestBuilder.Consts;
using QuarklessContexts.Models.Auth;
using QuarklessContexts.Models.Requests;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quarkless.Services.RequestBuilder
{
	public class RequestBuilder : IRequestBuilder
	{
		private IRestSharpClientManager _restSharpClient;
		public RequestBuilder(IRestSharpClientManager restSharpClient)
		{
			_restSharpClient = restSharpClient;
		}

		public IEnumerable<HttpHeader> DefaultHeaders(string instaUser, string token)
		{
			return new List<HttpHeader>()
			{
				new HttpHeader
				{
					Name = "FocusInstaAccount",
					Value = instaUser
				},
				new HttpHeader
				{
					Name = "Authorization",
					Value = token
				}
			};
		}

		public RestModel Generate<TJsonBody>(TJsonBody jsonBody, RequestType requestType, string url, string userId,
			List<HttpHeader> httpHeaders, IEnumerable<Parameter> parameters = null,
			string resourceAction = null)
		{
			return new RestModel
			{
				
				BaseUrl = url,
				JsonBody = JsonConvert.SerializeObject(jsonBody,Formatting.Indented),
				Parameters = parameters,
				RequestType = requestType,
				RequestHeaders = httpHeaders,
				ResourceAction = resourceAction
			};
		}
	}
}
