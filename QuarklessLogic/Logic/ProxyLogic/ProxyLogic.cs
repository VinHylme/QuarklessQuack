using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using QuarklessContexts.Models.Proxies;
using QuarklessLogic.Handlers.ReportHandler;
using QuarklessRepositories.ProxyRepository;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.ComponentModel;
using QuarklessContexts.Extensions;

namespace QuarklessLogic.Logic.ProxyLogic
{
	public enum ConnectionType
	{
		Any,

		[Description("Residential")]
		Residential,

		[Description("Mobile")]
		Mobile,

		[Description("Datacenter")]
		Datacenter
	}
	public struct IPResponse
	{
		[JsonProperty("ip")]
		public string IP;
	}
	public class ProxyItem
	{
		[JsonProperty("proxy")]
		public string Proxy { get; set; }

		[JsonProperty("ip")]
		public string IP { get; set; }

		[JsonProperty("port")]
		public string Port { get; set; }

		[JsonProperty("type")]
		public string Type { get; set; }

		[JsonProperty("lastChecked")]
		public int LastChecked { get; set; }

		[JsonProperty("get")]
		public bool Get { get; set; }

		[JsonProperty("post")]
		public bool Post { get; set; }

		[JsonProperty("cookies")]
		public bool Cookies { get; set; }

		[JsonProperty("referer")]
		public bool Referer { get; set; }

		[JsonProperty("userAgent")]
		public bool UserAgent { get; set; }

		[JsonProperty("city")]
		public string City { get; set; }

		[JsonProperty("state")]
		public string State { get; set; }

		[JsonProperty("country")]
		public string Country { get; set; }

		[JsonProperty("currentThreads")]
		public int CurrentThreads { get; set; }

		[JsonProperty("threadsAllowed")]
		public int ThreadsAllowed { get; set; }
	}

	public class ProxyLogic : IProxyLogic
	{
		private readonly IProxyRepostory _proxyRepostory;
		private readonly IReportHandler _reportHandler;
		public ProxyLogic(IProxyRepostory proxyRepostory, IReportHandler reportHandler)
		{
			_proxyRepostory = proxyRepostory;
			_reportHandler = reportHandler;
			_reportHandler.SetupReportHandler("/Logic/Proxy");
		}

		public bool AddProxies(List<ProxyModel> proxies)
		{
			try { 
				if(proxies.Any(s=>string.IsNullOrEmpty(s.Address))) return false;
				_proxyRepostory.AddProxies(proxies);
				return true;
			}
			catch(Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return false;
			}
		}

		public bool AddProxy(ProxyModel proxy)
		{
			try
			{
				_proxyRepostory.AddProxy(proxy);
				return true;
			}
			catch(Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return false;
			}
		}

		public async Task<bool> AssignProxy(AssignedTo assignedTo)
		{
			try
			{
				var results = await _proxyRepostory.AssignProxy(assignedTo);
				return results;
			}
			catch(Exception ee)
			{
				_reportHandler.MakeReport($"Failed to assign proxy to user: {assignedTo.Account_Id}, error: {ee}");
				return false;
			}
		}

		public async Task<IEnumerable<ProxyModel>> GetAllAssignedProxies()
		{
			var results = await _proxyRepostory.GetAllAssignedProxies();
			return results;
		}

		public static HttpClientHandler RegisterProxy(ProxyModel proxyDetails)
		{
			var proxy = new WebProxy(proxyDetails.Address, proxyDetails.Port)
			{
				//Address = new Uri($"http://{proxyDetails.Address}:{proxyDetails.Port}"), //i.e: http://1.2.3.4.5:8080
				BypassProxyOnLocal = false,
				UseDefaultCredentials = false
			};

			if (proxyDetails.Username != null && proxyDetails.Password != null)
			{
				proxy.Credentials = new NetworkCredential(userName: proxyDetails.Username, password: proxyDetails.Password);
			}
			var httpClientHandler = new HttpClientHandler()
			{
				Proxy = proxy,
			};

			if (proxyDetails.NeedServerAuth)
			{
				httpClientHandler.PreAuthenticate = true;
				httpClientHandler.UseDefaultCredentials = false;
				httpClientHandler.Credentials = new NetworkCredential(
					userName: proxyDetails.Username,
					password: proxyDetails.Password);
			}
			return httpClientHandler;
		}
		public async Task <bool> TestProxy(ProxyItem proxy)
		{
			try
			{
				var req = (HttpWebRequest)HttpWebRequest.Create("http://ip-api.com/json");
				req.Timeout = 4000;
				req.Proxy = new WebProxy($"http://{proxy.Proxy}/");

				var resp = await req.GetResponseAsync();
				var json = new StreamReader(resp.GetResponseStream()).ReadToEnd();

				var myip = (string)JObject.Parse(json)["query"];

				if (myip == proxy.IP)
				{
					return true;
				}
				return false;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				return false;
			}
		}

		public async Task<ProxyModel> GetProxyAssignedTo(string accountId, string instagramAccountId)
		{
			var results = await _proxyRepostory.GetAssignedProxyOf(accountId,instagramAccountId);
			if (results != null)
			{
				return results;
			}
			return null;
		}
		public async Task<ProxyModel> RetrieveRandomProxy(bool? get = null, bool? post = null, bool? cookies = null, bool? referer = null,
			bool? userAgent = null, int port = -1, string city = null, string state = null, string country  = null, 
			ConnectionType connectionType = ConnectionType.Any)
		{
			try
			{
				#region URL BUILD
				string baseUrl = $@"http://falcon.proxyrotator.com:51337/?apiKey=XR4E5JzkxMZcovaYQW2VUBw3PDj876eK";
				if (get != null)
					baseUrl += $"&get={get}";
				if (post != null)
					baseUrl += $"&post={post}";
				if (cookies != null)
					baseUrl += $"&cookies={cookies}";
				if (referer != null)
					baseUrl += $"&referer={referer}";
				if (userAgent != null)
					baseUrl += $"&userAgent={userAgent}";
				if (port != -1)
					baseUrl += $"&port={port}";
				if (!string.IsNullOrEmpty(city))
					baseUrl += $"&city={city}";
				if (!string.IsNullOrEmpty(state))
					baseUrl += $"&state={state}";
				if (!string.IsNullOrEmpty(country))
					baseUrl += $"&country={country}";
				if (connectionType != ConnectionType.Any)
					baseUrl += $"&connectionType={connectionType.GetDescription()}";
				#endregion

				var jsonRespoonse = string.Empty;
				ProxyItem proxyItem = new ProxyItem();
				HttpWebRequest request = (HttpWebRequest)WebRequest.Create(baseUrl);

				using (HttpWebResponse response = (HttpWebResponse) await request.GetResponseAsync())
				using (Stream stream = response.GetResponseStream())
				using (StreamReader reader = new StreamReader(stream))
				{
					jsonRespoonse = reader.ReadToEnd();
					proxyItem = JsonConvert.DeserializeObject<ProxyItem>(jsonRespoonse);
				}

				if (!string.IsNullOrEmpty(proxyItem?.Proxy) && !string.IsNullOrEmpty(proxyItem?.IP)) {

					if(await TestProxy(proxyItem))
					{
						return new ProxyModel{
							Address = proxyItem.IP,
							Port = int.Parse(proxyItem.Port),
							Region = proxyItem.Country,
							Type = proxyItem.Type
						};
					}
				}

				return await RetrieveRandomProxy(get, post, cookies, referer, userAgent, port, city, state, country, connectionType);
			}
			catch(Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<bool> RemoveUserFromProxy(AssignedTo assignedTo)
		{
			try
			{
				if ((await GetProxyAssignedTo(assignedTo.Account_Id, assignedTo.InstaId)) != null)
				{
					var results = await _proxyRepostory.RemoveUserFromProxy(assignedTo);
					return results;
				}
				return false;
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport($"Failed to assign proxy to user: {assignedTo.Account_Id}, error: {ee}");
				return false;
			}
		}
	}
}
