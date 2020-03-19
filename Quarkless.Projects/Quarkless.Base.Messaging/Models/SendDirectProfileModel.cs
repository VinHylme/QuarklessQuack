using System.Collections.Generic;
using Quarkless.Base.Messaging.Models.Interfaces;

namespace Quarkless.Base.Messaging.Models
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