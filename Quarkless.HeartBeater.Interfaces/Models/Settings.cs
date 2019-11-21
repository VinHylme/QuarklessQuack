using System;
using System.Collections.Generic;
using Quarkless.HeartBeater.Interfaces.Enums;

namespace Quarkless.HeartBeater.Interfaces.Models
{
	public class Settings : IExecuteSettings
	{
		public ActionExecuteType ActionExecute { get; set; }
		public List<Account> Accounts { get; set; }
		public TimeSpan HeartBeat { get; set; } = TimeSpan.FromSeconds(260);
		public Settings()
		{
			Accounts = new List<Account>();
		}
	}
}
