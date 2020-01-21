using System.Collections.Generic;
using Quarkless.Models.Messaging.Interfaces;

namespace Quarkless.Models.Messaging
{
	public class SendDirectProfileModel : IDirectMessageModel
	{
		public IEnumerable<string> Recipients { get; set; }
		public long userId { get; set; }
		public SendDirectProfileModel()
		{
			Recipients = new List<string>();
		}
	}
}