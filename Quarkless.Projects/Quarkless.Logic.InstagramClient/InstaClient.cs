using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using InstagramApiSharp.API;
using InstagramApiSharp.API.Builder;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Android.DeviceInfo;
using InstagramApiSharp.Enums;
using InstagramApiSharp.Logger;
using MihaZupan;
using Newtonsoft.Json;
using Quarkless.Models.InstagramClient;
using Quarkless.Models.InstagramClient.Interfaces;
using Quarkless.Models.Proxy;
using Quarkless.Models.Proxy.Enums;

namespace Quarkless.Logic.InstagramClient
{
	public class InstaClient : IInstaClient
	{
		private const InstaApiVersionType INSTAGRAM_VERSION 
			= InstaApiVersionType.Version123;

		private const string INSTAGRAM_BASE_URL = "https://i.instagram.com";

		public IInstaApi ReturnClient => api;
		private string StateString { get; set; }
		private IInstaApi api { get; set; }

		public IInstaClient Empty()
		{
			var instaApi = InstaApiBuilder.CreateBuilder()
			.UseLogger(new DebugLogger(LogLevel.All))
			.SetRequestDelay(RequestDelay.FromSeconds(0, 2))
			.Build();

			instaApi.SetApiVersion(INSTAGRAM_VERSION);
			return new InstaClient()
			{
				api = instaApi
			};
		}

		public IInstaClient Empty(ProxyModel proxy, bool genDevice = false)
		{
			var instaApi = InstaApiBuilder.CreateBuilder()
				.UseLogger(new DebugLogger(LogLevel.All))
				.SetRequestDelay(RequestDelay.FromSeconds(0, 2))
				.Build();

			if(proxy!=null)
				instaApi.UseHttpClientHandler(SetupClientHandler(proxy));

			if(genDevice)
				instaApi.SetDevice(AndroidDeviceGenerator.GetRandomAndroidDevice());

			instaApi.SetApiVersion(INSTAGRAM_VERSION);
			instaApi.HttpClient.BaseAddress = new Uri(INSTAGRAM_BASE_URL);
			return new InstaClient()
			{
				api = instaApi
			};
		}

		/*
		private HttpClient SetupProxy(ProxyModel proxy)
		{
			if (proxy == null)
				return new HttpClient(){BaseAddress = new Uri("https://i.instagram.com/") };
			if (string.IsNullOrEmpty(proxy.HostAddress) || proxy.Port == 0)
				return null;
			HttpClient client = null;

			switch (proxy.ProxyType)
			{
				case ProxyType.Http:
				{
					var handler = new HttpClientHandler
					{
						Proxy = new WebProxy($"{proxy.HostAddress}:{proxy.Port}", false),
					};
					if (!string.IsNullOrEmpty(proxy.Username) && !string.IsNullOrEmpty(proxy.Password))
						handler.Credentials = new NetworkCredential(proxy.Username, proxy.Password);
					client = new HttpClient(handler, true);
					break;
				}
				case ProxyType.Socks5:
				{
					var proxySettings = new HttpToSocks5Proxy(proxy.HostAddress, proxy.Port);

					if (!string.IsNullOrEmpty(proxy.Username) && !string.IsNullOrEmpty(proxy.Password))
						proxySettings.Credentials = new NetworkCredential(proxy.Username, proxy.Password);

					var httpClientHandler = new HttpClientHandler
					{
						Proxy = proxySettings,
					};

					client = new HttpClient(httpClientHandler, true);

					break;
				}
				default: return null;
			}
			
			client.BaseAddress = new Uri("https://i.instagram.com/");
			return client;
		}
		*/

		public HttpClientHandler SetupClientHandler(ProxyModel proxy)
		{
			HttpClientHandler clientHandler = null;

			if (string.IsNullOrEmpty(proxy.HostAddress) || proxy.Port == 0)
				return new HttpClientHandler();

			switch (proxy.ProxyType)
			{
				case ProxyType.Http:
				{
					var handler = new HttpClientHandler
					{
						Proxy = new WebProxy($"{proxy.HostAddress}:{proxy.Port}", false),
					};

					if (!string.IsNullOrEmpty(proxy.Username) && !string.IsNullOrEmpty(proxy.Password))
						handler.Credentials = new NetworkCredential(proxy.Username, proxy.Password);

					clientHandler = handler;
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

					clientHandler = handler;
					break;
				}
			}
			return clientHandler;
		}

		private IInstaApi CreateApiClient(UserSessionData userSession, ProxyModel proxy = null,
			LogLevel logLevel = LogLevel.Exceptions, IRequestDelay requestDelay = null)
		{
			if(userSession==null)
				throw new Exception("Username & Password field required");

			if (requestDelay == null)
				requestDelay = RequestDelay.FromSeconds(1, 2);

			var clientApi = InstaApiBuilder.CreateBuilder()
				.UseLogger(new DebugLogger(logLevel))
				.SetRequestDelay(requestDelay)
				.SetUser(userSession)
				.Build();

			if (proxy != null)
			{
				clientApi.UseHttpClientHandler(SetupClientHandler(proxy));
			}

			clientApi.SetApiVersion(INSTAGRAM_VERSION);
			clientApi.HttpClient.BaseAddress = new Uri(INSTAGRAM_BASE_URL);
			return clientApi;
		}
		public IInstaClient GetClientFromModel(InstagramClientAccount instagramAccount)
		{
			if (instagramAccount?.InstagramAccount == null)
			{
				throw new Exception("Model cannot be null");
			}
			if (instagramAccount.InstagramAccount?.State == null)
			{
				var instance = CreateApiClient(new UserSessionData
				{
					UserName = instagramAccount.InstagramAccount.Username,
					Password = instagramAccount.InstagramAccount.Password
				}, instagramAccount.Proxy);

				instance.SetDevice(AndroidDeviceGenerator.GetRandomAndroidDevice());

				var res = instance.LoginAsync().GetAwaiter().GetResult();

				if (!res.Succeeded) throw new Exception("Failed to login");

				var stateDataAsString = instance.GetStateDataAsString();

				return new InstaClient()
				{
					api = instance,
					StateString = stateDataAsString
				};
			}
			else
			{
				var instance = CreateApiClient(instagramAccount.InstagramAccount.State.UserSession,
					instagramAccount.Proxy);

				instance.SetDevice(instagramAccount.InstagramAccount.State.DeviceInfo);

				instance.LoadStateDataFromString(JsonConvert.SerializeObject(instagramAccount.InstagramAccount.State));

				if (instance.IsUserValidated() && instance.Username == instagramAccount.InstagramAccount.Username)
					return new InstaClient()
					{
						api = instance,
						StateString = JsonConvert.SerializeObject(instagramAccount.InstagramAccount.State),
					};

				instance.SetUser(new UserSessionData()
				{
					UserName = instagramAccount.InstagramAccount.Username, 
					Password = instagramAccount.InstagramAccount.Password
				});

				var res = instance.LoginAsync().GetAwaiter().GetResult();

				if (!res.Succeeded) throw new Exception("Failed to login");

				var stateDataAsString = instance.GetStateDataAsString();
				
				return new InstaClient()
				{
					api = instance,
					StateString = stateDataAsString
				};
			}
		}

		#region State Manager	
		public IInstaClient StateClient(string state)
		{
			if (string.IsNullOrEmpty(state))
			{
				throw new Exception("State cannot be null");
			}

			var anotherInstance = InstaApiBuilder.CreateBuilder()
				.UseLogger(new DebugLogger(LogLevel.All))
				.SetUser(UserSessionData.Empty)
				.SetRequestDelay(RequestDelay.FromSeconds(2,15))
				.Build();

			anotherInstance.LoadStateDataFromString(state);
				return new InstaClient() 
				{ 
					api = anotherInstance,
					StateString = state,
				};
		}
		public async Task<string> GetStateDataFromString()
		{
			StateString = await api.GetStateDataAsStringAsync();
			return StateString;
		}

		public async Task LoadStateDataFromStringAsync(string state)
			=> await api.LoadStateDataFromStringAsync(state);
		
		public async Task<IResult<InstaChallengeRequireVerifyMethod>> GetChallengeRequireVerifyMethodAsync(string username, string password)
		{
			api.SetDevice(AndroidDeviceGenerator.GetRandomAndroidDevice());
			api.SetUser(new UserSessionData
			{
				Password = password,
				UserName = username
			});
			api.SetApiVersion(INSTAGRAM_VERSION);
			return await api.GetChallengeRequireVerifyMethodAsync();
		}
		public async Task<IResult<InstaLoginResult>> SubmitChallengeCode(string username, string password, InstaChallengeLoginInfo instaChallengeLoginInfo, string code)
		{
			api.SetDevice(AndroidDeviceGenerator.GetRandomAndroidDevice());
			api.SetUser(new UserSessionData
			{
				Password = password,
				UserName = username
			});
			api.ChallengeLoginInfo = instaChallengeLoginInfo;
			api.SetApiVersion(INSTAGRAM_VERSION);
			return await api.VerifyCodeForChallengeRequireAsync(code);
		}
		
		#endregion

		public async Task<IResult<string>> TryLogin()
		{
			if (api == null)
				throw new Exception("Api Client is empty");
			if(string.IsNullOrEmpty(api.Username))
				throw new Exception("No User Found");

			var results = await api.LoginAsync();

			return results.Succeeded
				? new InstagramApiSharp.Classes.Result<string>(results.Succeeded,
					await api.GetStateDataAsStringAsync(), results.Info)

				: new InstagramApiSharp.Classes.Result<string>(results.Succeeded,
					null, results.Info);
		}

		public async Task<IResult<string>> TryLogin(string username, string password, AndroidDevice device, 
			ProxyModel proxy = null)
		{
			var instance = CreateApiClient(new UserSessionData
			{
				UserName = username,
				Password = password
			}, proxy);

			instance.SetDevice(device);

			var results = await instance.LoginAsync();
			return results.Succeeded 
				? new InstagramApiSharp.Classes.Result<string>(results.Succeeded, await instance.GetStateDataAsStringAsync(), results.Info) 
				: new InstagramApiSharp.Classes.Result<string>(results.Succeeded, null, results.Info);
		}
	}
}
