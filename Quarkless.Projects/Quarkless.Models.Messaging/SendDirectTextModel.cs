using System.Collections.Generic;
using Quarkless.Models.Messaging.Interfaces;

namespace Quarkless.Models.Messaging
{
	public class SendDirectTextModel : IDirectMessageModel
	{
		public IEnumerable<string> Recipients { get; set; }
		public IEnumerable<string> Threads { get; set; }
		public string TextMessage { get; set; }
		public SendDirectTextModel()
		{
			Recipients = new List<string>();
			Threads = new List<string>();
		}
	}
}