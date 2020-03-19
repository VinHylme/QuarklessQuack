using Quarkless.Models.Common.Enums;
using System.Collections.Generic;
using Quarkless.Common.Timeline.Models;
using RestSharp;

namespace Quarkless.Base.RequestBuilder.Models.Interfaces
{
	public interface IRequestBuilder
	{
		RestModel Generate<TJsonBody>(TJsonBody jsonBody, RequestType requestType, string url,string userId, List<HttpHeader> httpHeaders, IEnumerable<Parameter> parameters = null, string resourceAction = null);
		IEnumerable<HttpHeader> DefaultHeaders(string instaUser);
	}
}