using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Quarkless.HeartBeater.__Init__;
using Quarkless.HeartBeater.Interfaces.Enums;
using Quarkless.HeartBeater.Interfaces.Models;

namespace Quarkless.HeartBeater.UserInformation
{
	internal class Entry
	{
		static async Task Main(string[] args)
		{
			var services = new BuildServices().Build();

			var settings = new Settings
			{
				Accounts = new List<Account> {new Account("lemonkaces")}, 
				ActionExecute = ActionExecuteType.UserSelf
			};

			try
			{
				await services.Get<IInit>().Endeavor(settings);
			}
			catch (Exception err)
			{
				Console.WriteLine(err.Message);
			}
		}
	}
}