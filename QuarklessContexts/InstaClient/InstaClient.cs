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
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using InstagramApiSharp.Enums;

namespace QuarklessContexts.InstaClient
{
	public class InstaClient : IInstaClient
	{
		private IInstaApi _client;
		private static RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();
		private string StateString { get; set; }

		public InstaClient Empty()
		{
			if (_client != null) { return this;}
			_client = InstaApiBuilder.CreateBuilder()
			.UseLogger(new DebugLogger(LogLevel.All))
			.SetRequestDelay(RequestDelay.FromSeconds(0, 2))
			.Build();
			_client.SetApiVersion(InstaApiVersionType.Version100);
			return this;
		}

		public InstaClient Empty(ProxyModel proxy, bool genDevice = false)
		{
			//if(_client != null) return this;
			_client = InstaApiBuilder.CreateBuilder()
				.UseLogger(new DebugLogger(LogLevel.All))
				.SetRequestDelay(RequestDelay.FromSeconds(0, 2))
				.Build();

			if(genDevice)
				_client.SetDevice(AndroidDeviceGenerator.GetRandomAndroidDevice());

			_client.SetApiVersion(InstaApiVersionType.Version100);
			_client.UseHttpClientHandler(SetupProxy(proxy));
			return this;
		}
		public IInstaApi ReturnClient => _client;

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
				var instancenew = InstaApiBuilder.CreateBuilder()
					.UseLogger(new DebugLogger(LogLevel.All))
					.SetRequestDelay(RequestDelay.FromSeconds(1, 2))
					.Build();
				instancenew.SetDevice(AndroidDeviceGenerator.GetRandomAndroidDevice());
				instancenew.SetApiVersion(InstagramApiSharp.Enums.InstaApiVersionType.Version100);
				instancenew.SetUser(new UserSessionData { UserName = instagramAccount.InstagramAccount.Username, Password = instagramAccount.InstagramAccount.Password});
				if (instagramAccount.Proxy != null)
					instancenew.UseHttpClientHandler(SetupProxy(instagramAccount.Proxy));

				var res = instancenew.LoginAsync().GetAwaiter().GetResult();
				if (res.Succeeded)
				{
					var newstate = instancenew.GetStateDataAsString();
					return new InstaClient()
					{
						_client = instancenew,
						StateString = newstate
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

				instance.SetApiVersion(InstagramApiSharp.Enums.InstaApiVersionType.Version100);
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
						var newstate = instance.GetStateDataAsString();
						return new InstaClient()
						{
							_client = instance,
							StateString = newstate
						};
					}
					else
					{
						throw new Exception("Failed to login");
					}
				}

				return new InstaClient()
				{
					_client = instance,
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
					_client = anotherInstance,
				};
		}

		#region Util
		private int NextRandom(int max)
		{
			byte[] r = new byte[4];
			int value;
			do
			{
				rngCsp.GetBytes(r);
				value = BitConverter.ToInt32(r, 0) & Int32.MaxValue;
			} while (value >= max * (Int32.MaxValue / max));
			return value % max;
		}
		#endregion
		#region State Manager
		/// <summary>
		/// Set statestring variable
		/// </summary>
		public async Task<string> GetStateDataFromString()
		{
			StateString =	await _client.GetStateDataAsStringAsync();
			return StateString;
		}
		/// <summary>
		/// sets the state in client
		/// </summary>
		/// <param name="state"></param>
		public async void LoadStateDataFromStringAsync(string state)
		{
			await _client.LoadStateDataFromStringAsync(state);
		}
		public async Task<IResult<InstaChallengeRequireVerifyMethod>> GetChallengeRequireVerifyMethodAsync(string username, string password)
		{
			_client.SetDevice(AndroidDeviceGenerator.GetRandomAndroidDevice());
			_client.SetUser(new UserSessionData
			{
				Password = password,
				UserName = username
			});
			_client.SetApiVersion(InstagramApiSharp.Enums.InstaApiVersionType.Version100);
			return await _client.GetChallengeRequireVerifyMethodAsync();
		}
		public async Task<IResult<InstaLoginResult>> SubmitChallangeCode(string username, string password, InstaChallengeLoginInfo instaChallengeLoginInfo, string code)
		{
			_client.SetDevice(AndroidDeviceGenerator.GetRandomAndroidDevice());
			_client.SetUser(new UserSessionData
			{
				Password = password,
				UserName = username
			});
			_client.SetChallengeInfo(instaChallengeLoginInfo);
			_client.SetApiVersion(InstagramApiSharp.Enums.InstaApiVersionType.Version100);
			return await _client.VerifyCodeForChallengeRequireAsync(code);
		}
		#endregion


		public async Task<IResult<InstaLoginResult>> TryLogin(string username, string password)
		{
			UserSessionData userSessionData = new UserSessionData
			{
				UserName = username,
				Password = password
			};
			_client.SetUser(userSessionData);
			_client.SetApiVersion(InstagramApiSharp.Enums.InstaApiVersionType.Version100);

			return await _client.LoginAsync();
		}

	}
}
