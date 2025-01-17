﻿using System.Threading.Tasks;
using InstagramApiSharp.API;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Android.DeviceInfo;
using Quarkless.Base.Proxy.Models;

namespace Quarkless.Base.InstagramClient.Models.Interfaces
{
	public interface IInstaClient
	{
		IInstaClient Empty();
		IInstaClient Empty(UserSessionData userSessionData);
		IInstaApi ReturnClient {get; }
		IInstaClient StateClient(string state);
		Task<string> GetStateDataFromString();
		Task LoadStateDataFromStringAsync(string state);
		IInstaClient Empty(ProxyModel proxy, bool genDevice = false);
		IResult<IInstaClient> GetClientFromModel(InstagramClientAccount instagramAccount);
		Task<IResult<string>> TryLogin();
		Task<IResult<string>> TryLogin(string username, string password, AndroidDevice device, ProxyModel proxy = null);
	}
}