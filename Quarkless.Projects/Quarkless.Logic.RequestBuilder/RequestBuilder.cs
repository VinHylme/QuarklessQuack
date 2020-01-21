using Newtonsoft.Json;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.RequestBuilder.Interfaces;
using Quarkless.Models.RestSharpClientManager.Interfaces;
using Quarkless.Models.Timeline;
using RestSharp;
using System.Collections.Generic;

namespace Quarkless.Logic.RequestBuilder
{
	public class RequestBuilder : IRequestBuilder
	{
		private IRestSharpClientManager _restSharpClient;
		public RequestBuilder(IRestSharpClientManager restSharpClient)
		{
			_restSharpClient = restSharpClient;
		}

		public IEnumerable<HttpHeader> DefaultHeaders(string instaUser)
		{
			return new List<HttpHeader>()
			{
				new HttpHeader
				{
					Name = "FocusInstaAccount",
					Value = instaUser
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
