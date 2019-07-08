using Newtonsoft.Json;
using QuarklessContexts.Enums;
using QuarklessContexts.Models.Timeline;
using QuarklessLogic.Handlers.RestSharpClient;
using RestSharp;
using System.Collections.Generic;
namespace QuarklessLogic.Handlers.RequestBuilder.RequestBuilder
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
