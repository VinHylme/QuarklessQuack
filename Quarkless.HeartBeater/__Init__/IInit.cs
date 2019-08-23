using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Quarkless.HeartBeater.__Init__
{
	public struct Account
	{
		public string Username { get; set; }

		public Account(string username)
		{
			this.Username = username;
		}
	}
	public class Settings 
	{
		public List<Account> Accounts {get; set; }
		public TimeSpan FirstChamper { get; set; } = TimeSpan.FromMinutes(1);
		public TimeSpan HeartBeat { get; set; } = TimeSpan.FromSeconds(260);
		public Settings()
		{
			Accounts = new List<Account>();
		}
	}
	public interface IInit
	{
		Task Endeavor(Settings settings);
		Task Populator(Settings settings);
	}
}
