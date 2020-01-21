using System.Collections.Generic;
using InstagramApiSharp.Classes.Models;
using Quarkless.Models.Messaging.Interfaces;

namespace Quarkless.Models.Messaging
{
	public class SendDirectPhotoModel : IDirectMessageModel
	{
		public IEnumerable<string> Recipients { get; set; }
		public InstaImage Image { get; set; }
		public SendDirectPhotoModel()
		{
			Recipients = new List<string>();
		}
	}
}