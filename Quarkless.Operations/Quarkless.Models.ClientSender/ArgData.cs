using System;
using Quarkless.Models.ClientSender.Enums;
using Quarkless.Models.ClientSender.Interfaces;

namespace Quarkless.Models.ClientSender
{
	[Serializable]
	public class ArgData : ICommandArgs
	{
		public AvailableClient Client { get; set; }
		public ServiceTypes[] Services { get; set; }
		public bool UseLocal { get; set; }
	}
}