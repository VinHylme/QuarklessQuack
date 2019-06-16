using System.Collections.Generic;
using Quarkless.Queue.Jobs.JobOptions;
using Quarkless.Services.RequestBuilder.Consts;
using QuarklessContexts.Models.Auth;
using QuarklessContexts.Models.Requests;
using RestSharp;

namespace Quarkless.Services.RequestBuilder
{
	public interface IRequestBuilder
	{
		RestModel Generate<TJsonBody>(TJsonBody jsonBody, RequestType requestType, string url,string userId, List<HttpHeader> httpHeaders, IEnumerable<Parameter> parameters = null, string resourceAction = null);
		IEnumerable<HttpHeader> DefaultHeaders(string instaUser, string token);
	}
}