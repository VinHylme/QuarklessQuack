using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using QuarklessContexts.Models.Proxies;
using QuarklessLogic.Handlers.ReportHandler;
using QuarklessRepositories.ProxyRepository;

namespace QuarklessLogic.Logic.ProxyLogic
{
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
		public bool TestProxy(ProxyModel proxy)
		{
			try
			{
				var req = (HttpWebRequest)HttpWebRequest.Create("http://ip-api.com/json");
				req.Timeout = 5000;

				req.Proxy = new WebProxy()
				{
					BypassProxyOnLocal = false,
					Credentials = new NetworkCredential(proxy.Username, proxy.Password),
					Address = new Uri($"http://{proxy.Username}:{proxy.Password}")
				};

				var resp = req.GetResponse();
				var json = new StreamReader(resp.GetResponseStream()).ReadToEnd();

				var myip = (string)JObject.Parse(json)["query"];

				if (myip == proxy.Address)
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
	}
}
