using System.Collections.Generic;
using Quarkless.Utilities.Models.ProxyModels;

namespace Quarkless.Utlities.Logic.ProxyServices
{
	public class ProxyService
	{
		public static List<Proxy> PremiumProxyList = new List<Proxy>
		{
			new Proxy
			{
				IP = "194.36.142.190",
				Port = 61028,
				Username = "dbgcx",
				Password = "TFaFNSgR"
			}
		};

		public static List<Proxy> FreeProxyList = new List<Proxy>
		{
			new Proxy
			{
				IP = "192.241.122.25",
				Port = 8000,
				Username = "fY5AYx",
				Password = "rKzfGY"
			},
			new Proxy
			{
				IP = "185.168.248.121",
				Port = 8000,
				Username = "SkRrD0",
				Password = "FvhfFV"
			},
			new Proxy
			{
				IP = "213.232.69.27",
				Port = 8000,
				Username = "Cur7bt",
				Password = "cR0nvC"
			},
		};
	}
}
