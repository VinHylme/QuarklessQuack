using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Quarkless.Models.Proxy;
using Quarkless.Models.Proxy.Interfaces;

namespace Quarkless.Logic.Proxy
{
	public class ProxyRequest : ProxyLogic, IProxyRequest
	{
		private readonly string _url;
		public ProxyRequest(ProxyRequestOptions options, IProxyAssignmentsRepository repo) : base(repo)
		{
			if (options == null || string.IsNullOrEmpty(options.Url))
				throw new NullReferenceException();
			_url = options.Url;
		}

		public async Task<bool> TestConnectivity(ProxyModel proxy)
		{
			return await TestProxyConnectivity(proxy)!=null;
		}

		public async Task<ProxyModel> AssignProxy(string accountId, string instagramAccountId, string locationQuery)
		{
			using var httpClient = new HttpClient();
			var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, string.Format(_url, accountId, instagramAccountId, locationQuery));
			var results = await httpClient.SendAsync(httpRequestMessage);
			return results.Content == null 
				? null 
				: JsonConvert.DeserializeObject<ProxyModel>(await results.Content.ReadAsStringAsync());
		}
	}
}
