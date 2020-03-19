using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using MihaZupan;
using Newtonsoft.Json.Linq;
using Quarkless.Base.Proxy.Models;
using Quarkless.Base.Proxy.Models.Enums;
using Quarkless.Base.Proxy.Models.Interfaces;

namespace Quarkless.Base.Proxy.Logic
{
	public class ProxyLogic : IProxyLogic
	{
		#region Constants
		public const long MAX_SPEED_TIME = 135000;
		private const string MY_IP_URL = "http://ip-api.com/json";
		private const string INSTAGRAM_BASE_URL = "https://www.instagram.com";
		#endregion
		private readonly IProxyAssignmentsRepository _proxyAssignmentsRepository;

		public ProxyLogic(IProxyAssignmentsRepository proxyAssignmentsRepository)
			=> _proxyAssignmentsRepository = proxyAssignmentsRepository;

		public HttpClient GetHttpClient(ProxyModel proxy)
		{
			var client = new HttpClient();
			switch (proxy.ProxyType)
			{
				case ProxyType.Http:
				{
					var handler = new HttpClientHandler
					{
						Proxy = new WebProxy($"{proxy.HostAddress}:{proxy.Port}", false),
					};
					if (!string.IsNullOrEmpty(proxy.Username) && !string.IsNullOrEmpty(proxy.Password))
					{
						handler.Proxy.Credentials = new NetworkCredential(proxy.Username, proxy.Password);
						handler.Credentials = new NetworkCredential(proxy.Username, proxy.Password);
						handler.UseDefaultCredentials = true;
					}
					client = new HttpClient(handler);
					break;
				}
				case ProxyType.Socks5:
				{
					var proxySettings = new HttpToSocks5Proxy(proxy.HostAddress, proxy.Port);

					if (!string.IsNullOrEmpty(proxy.Username) && !string.IsNullOrEmpty(proxy.Password))
						proxySettings.Credentials = new NetworkCredential(proxy.Username, proxy.Password);

					var handler = new HttpClientHandler
					{
						Proxy = proxySettings
					};
					if (!string.IsNullOrEmpty(proxy.Username) && !string.IsNullOrEmpty(proxy.Password))
					{
						handler.UseDefaultCredentials = true;
					}
					client = new HttpClient(handler);
					break;
				}
			}

			return client;
		}

		public static async Task<string> TestConnectivity(ProxyModelShort proxy)
		{
			try
			{
				if (string.IsNullOrEmpty(proxy.HostAddress))
				{
					return string.Empty;
				}

				if (proxy.Port <= 0 || proxy.Port > 65535)
				{
					return string.Empty;
				}

				var client = new HttpClient();
				var myActualIpResponse = await client.GetStringAsync(MY_IP_URL);
				if (string.IsNullOrEmpty(myActualIpResponse)) return null;
				var myActualIp = (string)JObject.Parse(myActualIpResponse)["query"];
				if (string.IsNullOrEmpty(myActualIp)) return null;

				switch (proxy.ProxyType)
				{
					case ProxyType.Http:
					{
						var handler = new HttpClientHandler
						{
							Proxy = new WebProxy($"{proxy.HostAddress}:{proxy.Port}", false),
						};

						if (!string.IsNullOrEmpty(proxy.Username) && !string.IsNullOrEmpty(proxy.Password))
						{
							handler.Proxy.Credentials = new NetworkCredential(proxy.Username, proxy.Password);
							handler.Credentials = new NetworkCredential(proxy.Username, proxy.Password);
							handler.UseDefaultCredentials = true;
						}

						client = new HttpClient(handler);
						break;
					}
					case ProxyType.Socks5:
					{
						var proxySettings = new HttpToSocks5Proxy(proxy.HostAddress, proxy.Port);

						if (!string.IsNullOrEmpty(proxy.Username) && !string.IsNullOrEmpty(proxy.Password))
						{
							proxySettings.Credentials = new NetworkCredential(proxy.Username, proxy.Password);
						}

						var handler = new HttpClientHandler
						{
							Proxy = proxySettings
						};
						if (!string.IsNullOrEmpty(proxy.Username) && !string.IsNullOrEmpty(proxy.Password))
						{
							handler.UseDefaultCredentials = true;
						}
						client = new HttpClient(handler);
						break;
					}
				}

				client.Timeout = TimeSpan.FromSeconds(4.5);

				var ipResponse = await client.GetStringAsync(MY_IP_URL);

				if (string.IsNullOrEmpty(ipResponse))
					return null;

				var myIpNow = (string)JObject.Parse(ipResponse)["query"];

				return myIpNow != myActualIp ? myIpNow : null;
			}
			catch (Exception err)
			{
				Console.WriteLine(err.Message);
				return null;
			}
		}
		public async Task<string> TestProxyConnectivity(ProxyModel proxy)
		{
			try
			{
				var client = new HttpClient();
				var myActualIpResponse = await client.GetStringAsync(MY_IP_URL);
				if (string.IsNullOrEmpty(myActualIpResponse)) return null;
				var myActualIp = (string)JObject.Parse(myActualIpResponse)["query"];
				if (string.IsNullOrEmpty(myActualIp)) return null;

				client = GetHttpClient(proxy);

				client.Timeout = TimeSpan.FromSeconds(4.5);

				var ipResponse = await client.GetStringAsync(MY_IP_URL);

				if (string.IsNullOrEmpty(ipResponse))
					return null;

				var myIpNow = (string)JObject.Parse(ipResponse)["query"];

				return myIpNow != myActualIp ? myIpNow : null;
			}
			catch (Exception err)
			{
				Console.WriteLine(err.Message);
				return null;
			}
		}
		public async Task<double?> TestSpeedOfProxy(ProxyModel proxy)
		{
			var speed = 0.0;
			try
			{
				var client = GetHttpClient(proxy);
				client.Timeout = TimeSpan.FromSeconds(4.5);
				var timer = new Stopwatch();
				timer.Start();
				var results = await client.GetStringAsync(INSTAGRAM_BASE_URL);
				timer.Stop();
				speed = timer.Elapsed.TotalMilliseconds;
			}
			catch (Exception err)
			{
				Console.WriteLine(err.Message);
				return null;
			}

			return speed;
		}
		public async Task<bool> AssignProxy(ProxyModel proxy)
			=> await _proxyAssignmentsRepository.AddAssignedProxy(proxy);
		public Task<ProxyModel> ReassignProxy(string proxyId, ProxyModel newModel)
			=> _proxyAssignmentsRepository.ReassignProxy(proxyId, newModel);
		public Task<bool> DeleteProxyAssigned(string proxyId)
			=> _proxyAssignmentsRepository.DeleteProxyAssigned(proxyId);
		public Task<ProxyModel> GetProxyAssigned(string accountId, string instagramAccountId)
			=> _proxyAssignmentsRepository.GetProxyAssigned(accountId, instagramAccountId);

		public async Task<ProxyResponse> GetProxyAssignedShort(string accountId, string instagramAccountId)
		{
			var proxy = await _proxyAssignmentsRepository.GetProxyAssigned(accountId, instagramAccountId);
			if (proxy == null) return null;
			return new ProxyResponse
			{
				AccountId = proxy.AccountId,
				InstagramId = proxy.InstagramId,
				FromUser = proxy.FromUser,
				HostAddress = proxy.FromUser ? proxy.HostAddress : null,
				Port = proxy.FromUser ? proxy.Port : 0,
				Password = proxy.Password,
				ProxyId = proxy._id,
				Username = proxy.Username,
				Location = proxy.Location,
				ProxyType = (int) proxy.ProxyType
			};
		}
		public Task<ProxyModel> GetProxyAssigned(string instagramAccountId)
			=> _proxyAssignmentsRepository.GetProxyAssigned(instagramAccountId);
		public async Task<List<ProxyModel>> GetAllProxyAssigned()
			=> await _proxyAssignmentsRepository.GetAllProxyAssigned();
		public async Task<List<ProxyModel>> GetAllProxyAssigned(ProxyType type)
			=> await _proxyAssignmentsRepository.GetAllProxyAssigned(type);
		public async Task<List<ProxyModel>> GetAllProxyAssigned(string accountId)
			=> await _proxyAssignmentsRepository.GetAllProxyAssigned(accountId);
	}
}
