using System.Collections.Generic;
using Quarkless.Models.Messaging.Interfaces;

namespace Quarkless.Models.Messaging
{
	public class SendDirectLinkModel : IDirectMessageModel
	{
		public IEnumerable<string> Recipients { get; set; }
		public IEnumerable<string> Threads { get; set; }
		public string TextMessage { get; set; }
		public string Link { get; set; }
		public SendDirectLinkModel()
		{
			Recipients = new List<string>();
			Threads = new List<string>();
		}
	}
}