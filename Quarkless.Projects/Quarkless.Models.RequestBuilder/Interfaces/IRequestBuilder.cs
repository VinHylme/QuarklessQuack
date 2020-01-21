using Quarkless.Models.Common.Enums;
using System.Collections.Generic;
using RestSharp;
using Quarkless.Models.Timeline;

namespace Quarkless.Models.RequestBuilder.Interfaces
{
	public interface IRequestBuilder
	{
		RestModel Generate<TJsonBody>(TJsonBody jsonBody, RequestType requestType, string url,string userId, List<HttpHeader> httpHeaders, IEnumerable<Parameter> parameters = null, string resourceAction = null);
		IEnumerable<HttpHeader> DefaultHeaders(string instaUser);
	}
}