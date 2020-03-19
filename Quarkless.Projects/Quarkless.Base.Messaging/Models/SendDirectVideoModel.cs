using System.Collections.Generic;
using InstagramApiSharp.Classes.Models;
using Quarkless.Base.Messaging.Models.Interfaces;

namespace Quarkless.Base.Messaging.Models
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