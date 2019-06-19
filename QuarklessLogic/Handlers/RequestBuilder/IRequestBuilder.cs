using System.Collections.Generic;
using QuarklessContexts.Enums;
using QuarklessContexts.Models.Timeline;
using RestSharp;

namespace QuarklessLogic.Handlers.RequestBuilder.RequestBuilder
{
	public interface IRequestBuilder
	{
		RestModel Generate<TJsonBody>(TJsonBody jsonBody, RequestType requestType, string url,string userId, List<HttpHeader> httpHeaders, IEnumerable<Parameter> parameters = null, string resourceAction = null);
		IEnumerable<HttpHeader> DefaultHeaders(string instaUser, string token);
	}
}