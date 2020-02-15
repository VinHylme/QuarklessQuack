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
	public class ProxyRequest : ProxyLogic, IProxyRequest, IEventSubscriber<ProfilePublishEventModel>, IEventSubscriber<ProfileDeletedEventModel>
	{
		private readonly string _baseUrl;
		private readonly string _assignEndpoint = "assign-by-location/{0}/{1}/{2}";
		private readonly string _unassignEndpoint = "unassign/{0}";
		private readonly IGeoLocationHandler _geoLocationHandler;
		public ProxyRequest(ProxyRequestOptions options, IGeoLocationHandler geoLocationHandler,
			IProxyAssignmentsRepository repo) : base(repo)
		{
			if (options == null || string.IsNullOrEmpty(options.Url))
				throw new NullReferenceException();
			_baseUrl = options.Url;
			_geoLocationHandler = geoLocationHandler;
		}

		public async Task<bool> TestConnectivity(ProxyModel proxy)
		{
			return await TestProxyConnectivity(proxy)!=null;
		}

		public async Task<ProxyModel> AssignProxy(string accountId, string instagramAccountId, string locationQuery)
		{
			using var httpClient = new HttpClient();
			var requestUrl = _baseUrl + string.Format(_assignEndpoint, accountId, instagramAccountId,
				System.Web.HttpUtility.UrlEncode(locationQuery));
			var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, requestUrl);
			var results = await httpClient.SendAsync(httpRequestMessage);
			return results.Content == null 
				? null 
				: JsonConvert.DeserializeObject<ProxyModel>(await results.Content.ReadAsStringAsync());
		}
		public async Task<bool> UnAssignProxy(string instagramAccountId)
		{
			var proxyDetail = await GetProxyAssigned(instagramAccountId);
			if (proxyDetail == null) return false;
			
			if (proxyDetail.FromUser)
			{
				await DeleteProxyAssigned(proxyDetail._id);
			}

			using var httpClient = new HttpClient();
			var requestUrl = _baseUrl + string.Format(_unassignEndpoint, instagramAccountId);
			var httpRequestMessage = new HttpRequestMessage(HttpMethod.Delete, requestUrl);
			var results = await httpClient.SendAsync(httpRequestMessage);
			return results.Content != null && results.IsSuccessStatusCode;
		}

		public async Task Handle(ProfilePublishEventModel @event)
		{
			if (@event.Equals(null))
				return;

			if (@event.UserProxy != null)	// assign from user's proxy
			{
				await AssignProxy(new ProxyModel
				{
					AccountId = @event.Profile.Account_Id,
					InstagramId = @event.Profile.InstagramAccountId,
					AssignedDate = DateTime.UtcNow,
					HostAddress = @event.UserProxy.HostAddress,
					Port = @event.UserProxy.Port,
					Username = @event.UserProxy.Username,
					Password = @event.UserProxy.Password,
					ProxyType = @event.UserProxy.ProxyType,
					FromUser = true
				});

				return;
			}

			if (@event.Location == null)
			{
				if (string.IsNullOrEmpty(@event.IpAddress)) return;
				var location = await _geoLocationHandler.IpGeolocation.GetUserLocationFromIp(@event.IpAddress);
				await AssignProxy(@event.Profile.Account_Id, @event.Profile.InstagramAccountId, location.City);
				return;
			}

			if (!string.IsNullOrEmpty(@event.Location.Address))
			{
				await AssignProxy(@event.Profile.Account_Id, @event.Profile.InstagramAccountId, @event.Location.Address);
				return;
			}

			//location is from js returning the lat and lon
			if (@event.Location.Coordinates.Latitude > 0 || @event.Location.Coordinates.Latitude < 0)
			{
				var toLocal = await _geoLocationHandler.GeoNames
					.SearchNearbyPlace(@event.Location.Coordinates.Latitude, @event.Location.Coordinates.Longitude);

				if (toLocal.Geonames.Any())
					await AssignProxy(@event.Profile.Account_Id, @event.Profile.InstagramAccountId, toLocal.Geonames.First().Name);
			}
		}

		public async Task Handle(ProfileDeletedEventModel @event)
		{
			await UnAssignProxy(@event.InstagramAccountId);
		}
	}
}
