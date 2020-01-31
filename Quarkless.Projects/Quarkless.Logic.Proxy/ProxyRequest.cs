using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Quarkless.Events.Interfaces;
using Quarkless.Geolocation;
using Quarkless.Models.Profile;
using Quarkless.Models.Proxy;
using Quarkless.Models.Proxy.Interfaces;

namespace Quarkless.Logic.Proxy
{
	public class ProxyRequest : ProxyLogic, IProxyRequest, IEventSubscriber<ProfilePublishEventModel>
	{
		private readonly string _url;
		private readonly IGeoLocationHandler _geoLocationHandler;
		public ProxyRequest(ProxyRequestOptions options, IGeoLocationHandler geoLocationHandler, IProxyAssignmentsRepository repo) : base(repo)
		{
			if (options == null || string.IsNullOrEmpty(options.Url))
				throw new NullReferenceException();
			_url = options.Url;
			_geoLocationHandler = geoLocationHandler;
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

		public async Task Handle(ProfilePublishEventModel @event)
		{
			if (@event.Equals(null))
				return;

			//location is from js returning the lat and lon
			if (!string.IsNullOrEmpty(@event.Location))
			{
				var splitLoc = @event.Location.Split(",");
				var toLocal = await _geoLocationHandler.GeoNames
					.SearchNearbyPlace(double.Parse(splitLoc[0]), double.Parse(splitLoc[1]));

				if(toLocal.Geonames.Any())
					await AssignProxy(@event.Profile.Account_Id, @event.Profile.InstagramAccountId, toLocal.Geonames.First().Name);

				return;
			}

			if (!string.IsNullOrEmpty(@event.IpAddress))
			{
				var location = await _geoLocationHandler.IpGeolocation.GetUserLocationFromIp(@event.IpAddress);
				await AssignProxy(@event.Profile.Account_Id, @event.Profile.InstagramAccountId, location.City);
			}
		}
	}
}
