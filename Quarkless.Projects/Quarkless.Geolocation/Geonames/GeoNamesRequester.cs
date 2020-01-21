using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Quarkless.Geolocation.Geonames.Models;

namespace Quarkless.Geolocation.Geonames
{
	internal class GeoNamesRequester : IGeoNamesRequester
	{
		private const string BASE_URL = "http://api.geonames.org/";
		private const string FIND_NEARBY_PLACE = "findNearbyPlaceNameJSON?";
		private readonly string _token;
		internal GeoNamesRequester(string token)
		{
			_token = token;
		}
		public async Task<NearbyResponse> SearchNearbyPlace(double lat, double lng, int cityCount = 10, int radius = 10)
		{
			var url = BASE_URL + FIND_NEARBY_PLACE +
			          $"lat={lat}&lng={lng}&cities={cityCount}&radius={radius}&username={_token}";
			using (var httpClient = new HttpClient())
			{
				var res = await httpClient.GetStringAsync(url);
				if (string.IsNullOrEmpty(res))
					return null;
				try
				{
					return JsonConvert.DeserializeObject<NearbyResponse>(res);
				}
				catch
				{
					return null;
				}
			}
		}
	}
}
