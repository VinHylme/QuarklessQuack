using System.Collections.Generic;
using Quarkless.Base.Messaging.Models.Interfaces;

namespace Quarkless.Base.Messaging.Models
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