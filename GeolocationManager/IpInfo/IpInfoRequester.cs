using System;
using System.Net.Http;
using System.Threading.Tasks;
using GeolocationManager.IpInfo.Models;
using Newtonsoft.Json;

namespace GeolocationManager.IpInfo
{
	internal class IpInfoRequester : IIpInfoRequester
	{
		private readonly string _token;
		private readonly string _baseUrl;
		internal IpInfoRequester(string token)
		{
			_token = token;
			_baseUrl = "https://ipinfo.io/{0}?token=" + _token;
		}

		public async Task<IpGeolocation> GetUserLocationFromIp(string ip)
		{
			var url = string.Format(_baseUrl, ip);
			using var httpClient = new HttpClient();
			try
			{
				var response = await httpClient.GetStringAsync(url);
				return JsonConvert.DeserializeObject<IpGeolocation>(response);
			}
			catch (Exception err)
			{
				Console.WriteLine(err.Message);
				return null;
			}
		}
	}

}
