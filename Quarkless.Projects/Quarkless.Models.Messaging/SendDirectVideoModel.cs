using System.Collections.Generic;
using InstagramApiSharp.Classes.Models;
using Quarkless.Models.Messaging.Interfaces;

namespace Quarkless.Models.Messaging
{
	public class SendDirectVideoModel : IDirectMessageModel
	{
		public IEnumerable<string> Recipients { get; set; }
		public InstaVideoUpload Video { get; set; }
		public SendDirectVideoModel()
		{
			Recipients = new List<string>();
		}
	}
}