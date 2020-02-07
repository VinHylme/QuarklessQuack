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
		private const InstaApiVersionType INSTAGRAM_VERSION = InstaApiVersionType.Version123;
		private const string INSTAGRAM_BASE_URL = "https://i.instagram.com";

		public IInstaApi ReturnClient => instagramApi;
		private IInstaApi instagramApi { get; }
		private string StateString { get; set; }

		public InstaClient(IInstaApi instagramApi)
		{
			if (instagramApi == null)
				instagramApi = CreateEmptyClient();

			this.instagramApi = instagramApi;
		}

		public IInstaApi CreateEmptyClient()
		{
			var instaApi = InstaApiBuilder.CreateBuilder()
				.UseLogger(new DebugLogger(LogLevel.All))
				.SetRequestDelay(RequestDelay.FromSeconds(0, 2))
				.Build();
			instaApi.SetApiVersion(INSTAGRAM_VERSION);
			return instaApi;
		}

		public IInstaClient Empty()
		{
			var instaApi = CreateEmptyClient();
			return new InstaClient(instaApi);
		}
		public IInstaClient Empty(UserSessionData userSessionData)
		{
			var instaApi = CreateEmptyClient();
			instaApi.SetUser(userSessionData);
			return new InstaClient(instaApi);
		}
		public IInstaClient Empty(ProxyModel proxy, bool genDevice = false)
		{
			var instaApi = CreateEmptyClient();

			if(proxy!=null)
				instaApi.UseHttpClientHandler(SetupClientHandler(proxy));

			if(genDevice)
				instaApi.SetDevice(AndroidDeviceGenerator.GetRandomAndroidDevice());

			instaApi.HttpClient.BaseAddress = new Uri(INSTAGRAM_BASE_URL);
			return new InstaClient(instaApi);
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
		
		public IResult<IInstaClient> GetClientFromModel(InstagramClientAccount instagramAccount)
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

				var stateDataAsString = instance.GetStateDataAsString();
				var instaClient = new InstaClient(instance)
				{
					StateString = stateDataAsString
				};

				if (!res.Succeeded)
					return new InstagramApiSharp.Classes.Result<IInstaClient>(res.Succeeded, instaClient, res.Info);

				instaClient.StateString = instance.GetStateDataAsString();
				
				return new InstagramApiSharp.Classes.Result<IInstaClient>(res.Succeeded, instaClient, res.Info);
			}
			else
			{
				var instance = CreateApiClient(instagramAccount.InstagramAccount.State.UserSession,
					instagramAccount.Proxy);
				instance.SetDevice(instagramAccount.InstagramAccount.State.DeviceInfo);
				instance.LoadStateDataFromString(JsonConvert.SerializeObject(instagramAccount.InstagramAccount.State));

				if (instance.IsUserValidated() && instance.Username == instagramAccount.InstagramAccount.Username)
				{
					var client = new InstaClient(instance)
					{
						StateString = JsonConvert.SerializeObject(instagramAccount.InstagramAccount.State)
					};
					return new InstagramApiSharp.Classes.Result<IInstaClient>(true, client, null);
				}

				instance.SetUser(new UserSessionData
				{
					UserName = instagramAccount.InstagramAccount.Username, 
					Password = instagramAccount.InstagramAccount.Password
				});

				var stateDataAsString = instance.GetStateDataAsString();
				var instaClient = new InstaClient(instance)
				{
					StateString = stateDataAsString
				};

				var res = instance.LoginAsync().GetAwaiter().GetResult();
				if (!res.Succeeded)
					return new InstagramApiSharp.Classes.Result<IInstaClient>(res.Succeeded, instaClient, res.Info);

				instaClient.StateString = instance.GetStateDataAsString();

				return new InstagramApiSharp.Classes.Result<IInstaClient>(res.Succeeded, instaClient, res.Info);
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
				return new InstaClient(anotherInstance) 
				{ 
					StateString = state,
				};
		}
		public async Task<string> GetStateDataFromString()
		{
			StateString = await instagramApi.GetStateDataAsStringAsync();
			return StateString;
		}
		public async Task LoadStateDataFromStringAsync(string state)
			=> await instagramApi.LoadStateDataFromStringAsync(state);
		
		public async Task<IResult<InstaChallengeRequireVerifyMethod>> GetChallengeRequireVerifyMethodAsync(string username, string password)
		{
			instagramApi.SetDevice(AndroidDeviceGenerator.GetRandomAndroidDevice());
			instagramApi.SetUser(new UserSessionData
			{
				Password = password,
				UserName = username
			});
			instagramApi.SetApiVersion(INSTAGRAM_VERSION);
			return await instagramApi.GetChallengeRequireVerifyMethodAsync();
		}
		public async Task<IResult<InstaLoginResult>> SubmitChallengeCode(string username, string password, InstaChallengeLoginInfo instaChallengeLoginInfo, string code)
		{
			instagramApi.SetDevice(AndroidDeviceGenerator.GetRandomAndroidDevice());
			instagramApi.SetUser(new UserSessionData
			{
				Password = password,
				UserName = username
			});
			instagramApi.ChallengeLoginInfo = instaChallengeLoginInfo;
			instagramApi.SetApiVersion(INSTAGRAM_VERSION);
			return await instagramApi.VerifyCodeForChallengeRequireAsync(code);
		}
		
		#endregion
		public async Task<IResult<string>> TryLogin()
		{
			if (instagramApi == null)
				throw new Exception("Api Client is empty");
			if(string.IsNullOrEmpty(instagramApi.Username))
				throw new Exception("No User Found");

			var results = await instagramApi.LoginAsync();

			return results.Succeeded
				? new InstagramApiSharp.Classes.Result<string>(results.Succeeded,
					await instagramApi.GetStateDataAsStringAsync(), results.Info)

				: new InstagramApiSharp.Classes.Result<string>(results.Succeeded,
					null, results.Info);
		}

		public async Task<IResult<string>> TryLogin(string username, string password, AndroidDevice device, 
			ProxyModel proxy = null)
		{
			var instance = instagramApi ?? CreateApiClient(new UserSessionData
			{
				UserName = username,
				Password = password
			}, proxy);

			instance.SetDevice(device ?? AndroidDeviceGenerator.GetRandomAndroidDevice());

			var results = await instance.LoginAsync();
			return results.Succeeded 
				? new InstagramApiSharp.Classes.Result<string>(results.Succeeded, await instance.GetStateDataAsStringAsync(), results.Info) 
				: new InstagramApiSharp.Classes.Result<string>(results.Succeeded, null, results.Info);
		}
	}
}
