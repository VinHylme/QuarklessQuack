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
using Newtonsoft.Json;
using Quarkless.Models.InstagramClient;
using Quarkless.Models.InstagramClient.Interfaces;
using Quarkless.Models.Proxy;
using Quarkless.Models.Proxy.Enums;
using SocksSharp;
using SocksSharp.Proxy;

namespace Quarkless.Logic.InstagramClient
{
	public class InstaClient : IInstaClient
	{
		private const InstaApiVersionType INSTAGRAM_VERSION 
			= InstaApiVersionType.Version123;

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
				.UseHttpClient(proxy == null ? null : SetupProxy(proxy))
				.Build();

			if(genDevice)
				instaApi.SetDevice(AndroidDeviceGenerator.GetRandomAndroidDevice());

			instaApi.SetApiVersion(INSTAGRAM_VERSION);
			return new InstaClient()
			{
				api = instaApi
			};
		}

		private HttpClient SetupProxy(ProxyModel proxy)
		{
			if (proxy == null)
				return null;
			if (string.IsNullOrEmpty(proxy.HostAddress) || proxy.Port == 0)
				return null;
			HttpClient client = null;
			switch (proxy.ProxyType)
			{
				case ProxyType.Http:
					var handler = new HttpClientHandler
					{
						Proxy = new WebProxy($"{proxy.HostAddress}:{proxy.Port}", false),
					};

					if (!string.IsNullOrEmpty(proxy.Username) && !string.IsNullOrEmpty(proxy.Password))
						handler.Credentials = new NetworkCredential(proxy.Username, proxy.Password);
					client = new HttpClient(handler);
					break;
				case ProxyType.Socks5:
					var settings = new ProxySettings
					{
						ConnectTimeout = 4500,
						Host = proxy.HostAddress,
						Port = proxy.Port
					};

					if (!string.IsNullOrEmpty(proxy.Username) && !string.IsNullOrEmpty(proxy.Password))
						settings.Credentials = new NetworkCredential(proxy.Username, proxy.Password);

					client = new HttpClient(new ProxyClientHandler<Socks5>(settings));
					break;

			}
			return client;
		}
		public IInstaClient GetClientFromModel(InstagramClientAccount instagramAccount)
		{
			if (instagramAccount == null)
			{
				throw new Exception("Model cannot be null");
			}
			if (instagramAccount.InstagramAccount?.State == null)
			{
				var instanceNew = InstaApiBuilder.CreateBuilder()
					.UseLogger(new DebugLogger(LogLevel.All))
					.UseHttpClient(SetupProxy(instagramAccount.Proxy))
					.SetRequestDelay(RequestDelay.FromSeconds(1, 2))
					.Build();

				instanceNew.SetDevice(AndroidDeviceGenerator.GetRandomAndroidDevice());
				instanceNew.SetApiVersion(INSTAGRAM_VERSION);
				instanceNew.SetUser(new UserSessionData
				{
					UserName = instagramAccount.InstagramAccount?.Username, 
					Password = instagramAccount.InstagramAccount?.Password
				});
				
//				if (instagramAccount.Proxy != null)
//					instanceNew.UseHttpClientHandler(SetupProxy(instagramAccount.Proxy));

				var res = instanceNew.LoginAsync().GetAwaiter().GetResult();

				if (!res.Succeeded) throw new Exception("Failed to login");
				var stateDataAsString = instanceNew.GetStateDataAsString();
				return new InstaClient()
				{
					api = instanceNew,
					StateString = stateDataAsString
				};
			}
			else { 
				var instance = InstaApiBuilder.CreateBuilder()
					.UseLogger(new DebugLogger(LogLevel.Exceptions))
					.UseHttpClient(SetupProxy(instagramAccount.Proxy))
					.SetUser(instagramAccount.InstagramAccount.State.UserSession)
					.SetRequestDelay(RequestDelay.FromSeconds(1,2))
					.Build();

				instance.SetApiVersion(INSTAGRAM_VERSION);
				instance.SetDevice(instagramAccount.InstagramAccount.State.DeviceInfo);

//				if(instagramAccount.Proxy!=null)
//					instance.UseHttpClientHandler(SetupProxy(instagramAccount.Proxy));

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
		public async void LoadStateDataFromStringAsync(string state)
		{
			await api.LoadStateDataFromStringAsync(state);
		}
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

		public async Task<IResult<string>> TryLogin(string username, string password, AndroidDevice device, ProxyModel proxy = null)
		{
			var instanceNew = InstaApiBuilder.CreateBuilder()
				.UseLogger(new DebugLogger(LogLevel.All))
				.UseHttpClient(SetupProxy(proxy))
				.SetRequestDelay(RequestDelay.FromSeconds(1, 2))
				.Build();

			instanceNew.SetDevice(device);
			instanceNew.SetApiVersion(INSTAGRAM_VERSION);
			instanceNew.SetUser(new UserSessionData
			{
				UserName = username,
				Password = password
			});

			var results = await instanceNew.LoginAsync();
			return results.Succeeded 
				? new InstagramApiSharp.Classes.Result<string>(results.Succeeded, await instanceNew.GetStateDataAsStringAsync(), results.Info) 
				: new InstagramApiSharp.Classes.Result<string>(results.Succeeded, null, results.Info);
		}
	}
}
