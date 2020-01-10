using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using QuarklessContexts.Models.Proxies;
using QuarklessLogic.Handlers.ReportHandler;
using QuarklessRepositories.ProxyRepository;
using System.Linq;
using System.Net.Http;
using QuarklessContexts.Models.Profiles;
using QuarklessLogic.Handlers.EventHandlers;
using SocksSharp;
using SocksSharp.Proxy;

namespace QuarklessLogic.Logic.ProxyLogic
{
	public class ProxyLogic : IProxyLogic, IEventSubscriber<ProfilePublishEventModel>
	{
		private readonly IProxyRepository _proxyRepository;
		private readonly IReportHandler _reportHandler;
		private const string MY_IP_URL = "http://ip-api.com/json";
		public ProxyLogic(IProxyRepository proxyRepository, IReportHandler reportHandler)
		{
			_proxyRepository = proxyRepository;
			_reportHandler = reportHandler;
			_reportHandler.SetupReportHandler("/Logic/Proxy");
		}

		public bool AddProxies(List<ProxyModel> proxies)
		{
			try
			{
				if (proxies.Any(s => string.IsNullOrEmpty(s.Address))) return false;
				_proxyRepository.AddProxies(proxies);
				return true;
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return false;
			}
		}
		public bool AddProxy(ProxyModel proxy)
		{
			try
			{
				_proxyRepository.AddProxy(proxy);
				return true;
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return false;
			}
		}


		public async Task<bool> AssignSocks5Proxy(string accountId, string instagramAccountId, string userIp)
		{
			//todo: get location from user ip, and then find nearest proxy server for that location
			//todo: once retrieved then test the proxy a few times, check if the ip is different
			//todo: once successful, register the new proxy to the user
			//todo: will need to test the proxy every day
			return false;
		}

		//todo: change assign logic to work with the socks5 
		//todo: user registers => take location => assign nearest location available for that user
		//todo: think about when there are no proxies available 
		public async Task<bool> AssignProxy(AssignedTo assignedTo)
		{
			try
			{
				var results = await _proxyRepository.AssignProxy(assignedTo);
				return results;
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport($"Failed to assign proxy to user: {assignedTo.Account_Id}, error: {ee}");
				return false;
			}
		}
		public async Task<IEnumerable<ProxyModel>> GetAllAssignedProxies()
		{
			var results = await _proxyRepository.GetAllAssignedProxies();
			return results;
		}
		public async Task<ProxyModel> GetProxyAssignedTo(string accountId, string instagramAccountId)
		{
			var results = await _proxyRepository.GetAssignedProxyOf(accountId, instagramAccountId);
			return results;
		}

		public async Task<bool> RemoveUserFromProxy(AssignedTo assignedTo)
		{
			try
			{
				if ((await GetProxyAssignedTo(assignedTo.Account_Id, assignedTo.InstaId)) == null)
					return false;
				var results = await _proxyRepository.RemoveUserFromProxy(assignedTo);
				return results;
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport($"Failed to assign proxy to user: {assignedTo.Account_Id}, error: {ee}");
				return false;
			}
		}

		public async Task<bool> TestProxy(ProxyModel proxy)
		{
			try
			{
				var client = new HttpClient();
				var myActualIpResponse = await client.GetStringAsync(MY_IP_URL);
				if (string.IsNullOrEmpty(myActualIpResponse)) return false;
				var myActualIp = (string) JObject.Parse(myActualIpResponse)["query"];
				if (string.IsNullOrEmpty(myActualIp)) return false;

				switch (proxy.Type)
				{
					case ProxyType.Http:
						var handler = new HttpClientHandler
						{
							Proxy = new WebProxy($"{proxy.Address}:{proxy.Port}", false),
						};

						if (!string.IsNullOrEmpty(proxy.Username) && !string.IsNullOrEmpty(proxy.Password))
							handler.Credentials = new NetworkCredential(proxy.Username, proxy.Password);
						client = new HttpClient(handler);
						break;
					case ProxyType.Socks5:
						var settings = new ProxySettings
						{
							ConnectTimeout = 4500,
							Host = proxy.Address,
							Port = proxy.Port
						};
						
						if(!string.IsNullOrEmpty(proxy.Username) && !string.IsNullOrEmpty(proxy.Password))
							settings.Credentials = new NetworkCredential(proxy.Username, proxy.Password);
						
						client = new HttpClient(new ProxyClientHandler<Socks5>(settings));
						break;

				}

				client.Timeout = TimeSpan.FromSeconds(4.5);

				var ipResponse = await client.GetStringAsync(MY_IP_URL);
				if (string.IsNullOrEmpty(ipResponse))
					return false;
				var myIpNow = (string) JObject.Parse(ipResponse)["query"];
				
				return myIpNow != myActualIp;
			}
			catch(Exception err)
			{
				_reportHandler.MakeReport(err);
				Console.WriteLine(err.Message);
				return false;
			}
		}
		public async Task Handle(ProfilePublishEventModel @event)
		{
			await AssignProxy(new AssignedTo
			{
				Account_Id = @event.Profile.Account_Id, 
				InstaId = @event.Profile.InstagramAccountId
			});
		}
	}
}
