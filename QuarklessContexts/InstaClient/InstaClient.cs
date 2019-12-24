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
		public IInstaApi ReturnClient { get; private set; }
		private const InstaApiVersionType INSTAGRAM_VERSION = InstaApiVersionType.Version123;

		private string StateString { get; set; }

		public InstaClient Empty()
		{
			if (ReturnClient != null) { return this;}
			ReturnClient = InstaApiBuilder.CreateBuilder()
			.UseLogger(new DebugLogger(LogLevel.All))
			.SetRequestDelay(RequestDelay.FromSeconds(0, 2))
			.Build();
			ReturnClient.SetApiVersion(INSTAGRAM_VERSION);
			return this;
		}

		public InstaClient Empty(ProxyModel proxy, bool genDevice = false)
		{
			//if(_client != null) return this;
			ReturnClient = InstaApiBuilder.CreateBuilder()
				.UseLogger(new DebugLogger(LogLevel.All))
				.SetRequestDelay(RequestDelay.FromSeconds(0, 2))
				.Build();

			if(genDevice)
				ReturnClient.SetDevice(AndroidDeviceGenerator.GetRandomAndroidDevice());

			ReturnClient.SetApiVersion(INSTAGRAM_VERSION);
			ReturnClient.UseHttpClientHandler(SetupProxy(proxy));
			return this;
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
				if (res.Succeeded)
				{
					var stateDataAsString = instanceNew.GetStateDataAsString();
					return new InstaClient()
					{
						ReturnClient = instanceNew,
						StateString = stateDataAsString
					};
				}
				else
				{
					throw new Exception("Failed to login");
				}
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

				if (!instance.IsUserValidated() || instance.Username!=instagramAccount.InstagramAccount.Username)
				{
					instance.SetUser(new UserSessionData() { UserName = instagramAccount.InstagramAccount.Username, Password = instagramAccount.InstagramAccount.Password});
					var res = instance.LoginAsync().GetAwaiter().GetResult();
					if (res.Succeeded)
					{
						var stateDataAsString = instance.GetStateDataAsString();
						return new InstaClient()
						{
							ReturnClient = instance,
							StateString = stateDataAsString
						};
					}
					else
					{
						throw new Exception("Failed to login");
					}
				}

				return new InstaClient()
				{
					ReturnClient = instance,
					StateString = JsonConvert.SerializeObject(instagramAccount.InstagramAccount.State),
				};
			}
		}
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
					StateString = state,
					ReturnClient = anotherInstance,
				};
		}
		#region State Manager
		/// <summary>
		/// Set statestring variable
		/// </summary>
		public async Task<string> GetStateDataFromString()
		{
			StateString =	await ReturnClient.GetStateDataAsStringAsync();
			return StateString;
		}
		/// <summary>
		/// sets the state in client
		/// </summary>
		/// <param name="state"></param>
		public async void LoadStateDataFromStringAsync(string state)
		{
			await ReturnClient.LoadStateDataFromStringAsync(state);
		}
		public async Task<IResult<InstaChallengeRequireVerifyMethod>> GetChallengeRequireVerifyMethodAsync(string username, string password)
		{
			ReturnClient.SetDevice(AndroidDeviceGenerator.GetRandomAndroidDevice());
			ReturnClient.SetUser(new UserSessionData
			{
				Password = password,
				UserName = username
			});
			ReturnClient.SetApiVersion(INSTAGRAM_VERSION);
			return await ReturnClient.GetChallengeRequireVerifyMethodAsync();
		}
		public async Task<IResult<InstaLoginResult>> SubmitChallangeCode(string username, string password, InstaChallengeLoginInfo instaChallengeLoginInfo, string code)
		{
			ReturnClient.SetDevice(AndroidDeviceGenerator.GetRandomAndroidDevice());
			ReturnClient.SetUser(new UserSessionData
			{
				Password = password,
				UserName = username
			});
			ReturnClient.ChallengeLoginInfo = instaChallengeLoginInfo;
			ReturnClient.SetApiVersion(INSTAGRAM_VERSION);
			return await ReturnClient.VerifyCodeForChallengeRequireAsync(code);
		}
		#endregion


		public async Task<IResult<InstaLoginResult>> TryLogin(string username, string password)
		{
			UserSessionData userSessionData = new UserSessionData
			{
				UserName = username,
				Password = password
			};
			ReturnClient.SetUser(userSessionData);
			ReturnClient.SetApiVersion(INSTAGRAM_VERSION);

			return await ReturnClient.LoginAsync();
		}

	}
}
