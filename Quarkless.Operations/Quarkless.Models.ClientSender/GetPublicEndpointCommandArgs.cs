using System;
using Quarkless.Models.ClientSender.Interfaces;

namespace Quarkless.Models.ClientSender
{
	[Serializable]
	public class GetPublicEndpointCommandArgs : ICommandArgs
	{
		public bool GetAll { get; set; } = true;
		public bool UseLocal { get; set; }
	}
}