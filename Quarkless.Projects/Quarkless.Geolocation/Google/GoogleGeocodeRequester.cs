using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Quarkless.Geolocation.Google.Models;

namespace Quarkless.Geolocation.Google
{

	internal class GoogleGeocodeRequester : IGoogleGeocodeRequester
	{
		private const string GOOGLE_MAPS_API = "https://maps.googleapis.com/maps/api/";
		private const string GOOGLE_GEOCODE_API = GOOGLE_MAPS_API + "geocode/json?";
		private const string GOOGLE_PLACES_TEXT_API = GOOGLE_MAPS_API + "place/textsearch/json?";
		private const string GOOGLE_PLACES_AUTOCOMPLETE_API = GOOGLE_MAPS_API + "place/autocomplete/json?";
		private const string KEY = "&key={0}";
		private readonly string _tokenKey;
		internal GoogleGeocodeRequester(string token)
		{
			_tokenKey = token;
		}

		public async Task<GeocodeResponse> GetGeoInformation(double lat, double lng)
		{
			var query = GOOGLE_GEOCODE_API + $"latlng={lat},{lng}" + string.Format(KEY, _tokenKey);
			using (var httpClient = new HttpClient())
			{
				var res = await httpClient.GetStringAsync(query);
				return string.IsNullOrEmpty(res) ? null : JsonConvert.DeserializeObject<GeocodeResponse>(res);
			}
		}
		public async Task<GeocodeResponse> GetGeoInformation(string address)
		{
			var query = GOOGLE_GEOCODE_API + $"address={address}" + string.Format(KEY, _tokenKey);
			using (var httpClient = new HttpClient())
			{
				var res = await httpClient.GetStringAsync(query);
				return string.IsNullOrEmpty(res) ? null : JsonConvert.DeserializeObject<GeocodeResponse>(res);
			}
		}
	}
}
