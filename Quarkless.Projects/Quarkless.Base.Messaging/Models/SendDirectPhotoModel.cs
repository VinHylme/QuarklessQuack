using System.Collections.Generic;
using InstagramApiSharp.Classes.Models;
using Quarkless.Base.Messaging.Models.Interfaces;

namespace Quarkless.Base.Messaging.Models
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