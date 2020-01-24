using System;
using Quarkless.SmsHandler.Models;
using Quarkless.SmsHandler.Models.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Quarkless.SmsHandler.Logic
{
	public class SmsClient
	{
		#region Constants
		private const string API_KEY = "7a757e904d2024f52613f8706f42a8f7";
		private const string BASE_URL_ONLINE_SIM_RU = "http://onlinesim.ru/api/";
		private string SERVICE_LIST = $"{BASE_URL_ONLINE_SIM_RU}getServiceList.php?apikey={API_KEY}"; //GET
		private string GET_NUMBER = $"{BASE_URL_ONLINE_SIM_RU}getNum.php"; //POST FORM DATA
		private string GET_STATUS = $"{BASE_URL_ONLINE_SIM_RU}getState.php?apikey={API_KEY}&tzid="; //GET (ADD tzid) -> received from getNum POST METHOD
		private string GET_BALANCE = $"{BASE_URL_ONLINE_SIM_RU}getBalance.php?apikey={API_KEY}";
		private string GET_OPERATIONS = $"{BASE_URL_ONLINE_SIM_RU}getOperations.php?apikey={API_KEY}";
		#endregion

		#region Russian Spy Stuff
		//uk = 11p - 18p, china = 2p - 8p
		public async Task<List<Status>> GetCurrentOperations(SmsServiceType serviceType)
		{
			var cookieContainer = new CookieContainer();
			var msgHandler = new HttpClientHandler
			{
				AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
				CookieContainer = cookieContainer
			};
			using (var client = new HttpClient(msgHandler, false))
			{
				var resp = await client.GetAsync(new Uri(GET_OPERATIONS));
				var body = await resp.Content.ReadAsStringAsync();
				if (body.Contains("ERROR_NO_OPERATIONS"))
					return new List<Status>();
				try
				{
					var statuses = JsonConvert.DeserializeObject<IEnumerable<Status>>(body);
					return statuses.Where(_=>_.Service.Equals(serviceType.ToString())).ToList();
				}
				catch (Exception ee)
				{
					return new List<Status>();
				}
			}
		}
		public async Task<GetNumberResponse> IssueNumberForService(CountryCode country = CountryCode.UnitedKingdom,
			SmsServiceType serviceType = SmsServiceType.Google)
		{
			var cookieContainer = new CookieContainer();
			var msgHandler = new HttpClientHandler
			{
				AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
				CookieContainer = cookieContainer
			};
			var formData = new FormUrlEncodedContent(new[]
			{
				new KeyValuePair<string, string>("apikey", API_KEY),
				new KeyValuePair<string, string>("service", serviceType.ToString()),
				new KeyValuePair<string, string>("form", "1"),
				new KeyValuePair<string, string>("country", ((int) country).ToString())
			});
			using (var client = new HttpClient(msgHandler, false))
			{
				try
				{
					var resp = await client.PostAsync(new Uri(GET_NUMBER), formData);
					var numberResp =
						JsonConvert.DeserializeObject<GetNumberResponse>(await resp.Content.ReadAsStringAsync());
					return numberResp;
				}
				catch
				{
					return null;
				}
			}
		}
		public async Task<List<StatusMulti>> GetNumberStatus(int tzid)
		{
			var cookieContainer = new CookieContainer();
			var msgHandler = new HttpClientHandler
			{
				AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
				CookieContainer = cookieContainer
			};
			using (var client = new HttpClient(msgHandler, false))
			{
				var resp = await client.GetAsync(new Uri(GET_STATUS + tzid+ "&msg_list=1"));
				var body = await resp.Content.ReadAsStringAsync();
				try
				{
					var status = JsonConvert.DeserializeObject<IEnumerable<StatusMulti>>(body);
					return status.ToList();
				}
				catch (Exception ee)
				{
					return null;
				}
			}
		}
		public async Task<BalanceResponse> GetBalance()
		{
			var cookieContainer = new CookieContainer();
			var msgHandler = new HttpClientHandler
			{
				AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
				CookieContainer = cookieContainer
			};
			using (var client = new HttpClient(msgHandler, false))
			{
				var resp = await client.GetAsync(new Uri(GET_BALANCE));
				var body = await resp.Content.ReadAsStringAsync();
				try
				{
					var serialise = JsonConvert.DeserializeObject<BalanceResponse>(body);
					return serialise;
				}
				catch (Exception ee)
				{
					return null;
				}
			}
		}
		#endregion
	}
}
