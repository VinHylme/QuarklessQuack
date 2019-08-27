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

	public enum ActionExecuteType
	{
		Base,
		UserSelf,
		Other,
		TargetList
	}
	public class Settings 
	{
		public ActionExecuteType ActionExecute { get; set; }
		public List<Account> Accounts {get; set; }
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
		Task Creator();
	}
}
