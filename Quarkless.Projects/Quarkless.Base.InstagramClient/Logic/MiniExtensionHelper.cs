using System;
using InstagramApiSharp.Classes;
using Newtonsoft.Json;

namespace Quarkless.Base.InstagramClient.Logic
{
	public static class MiniExtensionHelper
	{
		public static bool SameAs(this StateData self, StateData target)
		{
			try
			{
				if (self == null || target == null)
					return false;

				var isDeviceSame = JsonConvert.SerializeObject(self.DeviceInfo)
					.Equals(JsonConvert.SerializeObject(target.DeviceInfo));

				var isAuthSame = self.IsAuthenticated.Equals(target.IsAuthenticated);

				var isRawCookiesSame = JsonConvert.SerializeObject(self.RawCookies)
					.Equals(JsonConvert.SerializeObject(target.RawCookies));

				var isUserSessionSame = JsonConvert.SerializeObject(self.UserSession)
					.Equals(JsonConvert.SerializeObject(target.UserSession));

				return isUserSessionSame && isAuthSame && isDeviceSame && isRawCookiesSame;
			}
			catch (Exception ee)
			{
				Console.WriteLine(ee.Message);
				return false;
			}
		}
	}
}