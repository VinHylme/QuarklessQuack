using System.Collections.Generic;

namespace Quarkless.HeartBeater.Interfaces.Models
{
	public class CorpusSettings : IExecuteSettings
	{
		public List<Account> Accounts { get; set; }
		public bool PerformCleaning { get; set; }
	}
}
