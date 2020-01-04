using InstagramApiSharp.API;
using InstagramApiSharp.API.Builder;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Android.DeviceInfo;
using InstagramApiSharp.Logger;
using Newtonsoft.Json;
using QuarklessContexts.Models.InstagramAccounts;
using QuarklessContexts.Models.Proxies;
using System;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using InstagramApiSharp.Enums;

namespace QuarklessContexts.InstaClient
{
	public class InstaClient : IInstaClient
	{
		private const InstaApiVersionType INSTAGRAM_VERSION = InstaApiVersionType.Version123;
		public IInstaApi ReturnClient => api;
		private string StateString { get; set; }
		private IInstaApi api { get; set; }

		public InstaClient Empty()
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
		public InstaClient Empty(ProxyModel proxy, bool genDevice = false)
		{
			var instaApi = InstaApiBuilder.CreateBuilder()
				.UseLogger(new DebugLogger(LogLevel.All))
				.SetRequestDelay(RequestDelay.FromSeconds(0, 2))
				.Build();

			if(genDevice)
				instaApi.SetDevice(AndroidDeviceGenerator.GetRandomAndroidDevice());

			instaApi.SetApiVersion(INSTAGRAM_VERSION);
			instaApi.UseHttpClientHandler(SetupProxy(proxy));
			return new InstaClient()
			{
				api = instaApi
			};
		}

		private HttpClientHandler SetupProxy(ProxyModel proxyModel)
		{
			var proxy = new WebProxy(Regex.Replace(proxyModel.Address,"[http://|https://]",""), proxyModel.Port)
			{
				BypassProxyOnLocal = false,
				UseDefaultCredentials = false
			};

			if (proxyModel.Username != null && proxyModel.Password != null)
			{
				proxy.Credentials = new NetworkCredential(userName: proxyModel.Username, password: proxyModel.Password);
			}

			var httpClientHandler = new HttpClientHandler()
			{
				Proxy = proxy,
			};


			return httpClientHandler;
		}
		public InstaClient GetClientFromModel(InstagramClientAccount instagramAccount)
		{
			if (instagramAccount == null)
			{
				throw new Exception("Model cannot be null");
			}
			if (instagramAccount.InstagramAccount?.State == null)
			{
				var instanceNew = InstaApiBuilder.CreateBuilder()
					.UseLogger(new DebugLogger(LogLevel.All))
					.SetRequestDelay(RequestDelay.FromSeconds(1, 2))
					.Build();

				instanceNew.SetDevice(AndroidDeviceGenerator.GetRandomAndroidDevice());
				instanceNew.SetApiVersion(INSTAGRAM_VERSION);
				instanceNew.SetUser(new UserSessionData
				{
					UserName = instagramAccount.InstagramAccount?.Username, 
					Password = instagramAccount.InstagramAccount?.Password
				});

				if (instagramAccount.Proxy != null)
					instanceNew.UseHttpClientHandler(SetupProxy(instagramAccount.Proxy));

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
					.SetUser(instagramAccount.InstagramAccount.State.UserSession)
					.SetRequestDelay(RequestDelay.FromSeconds(1,2))
					.Build();

				instance.SetApiVersion(INSTAGRAM_VERSION);
				instance.SetDevice(instagramAccount.InstagramAccount.State.DeviceInfo);

				if(instagramAccount.Proxy!=null)
					instance.UseHttpClientHandler(SetupProxy(instagramAccount.Proxy));

				instance.LoadStateDataFromString(JsonConvert.SerializeObject(instagramAccount.InstagramAccount.State));

				if (instance.IsUserValidated() && instance.Username == instagramAccount.InstagramAccount.Username)
					return new InstaClient()
					{
						api = instance,
						StateString = JsonConvert.SerializeObject(instagramAccount.InstagramAccount.State),
					};
				instance.SetUser(new UserSessionData() { UserName = instagramAccount.InstagramAccount.Username, Password = instagramAccount.InstagramAccount.Password});
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
		public InstaClient StateClient(string state)
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
		public async Task<IResult<InstaLoginResult>> SubmitChallangeCode(string username, string password, InstaChallengeLoginInfo instaChallengeLoginInfo, string code)
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

		public async Task<IResult<string>> TryLogin(string username, string password, AndroidDevice device)
		{
			var instanceNew = InstaApiBuilder.CreateBuilder()
				.UseLogger(new DebugLogger(LogLevel.All))
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
				? new Result<string>(results.Succeeded, await instanceNew.GetStateDataAsStringAsync(), results.Info) 
				: new Result<string>(results.Succeeded, null, results.Info);
		}
	}
}
