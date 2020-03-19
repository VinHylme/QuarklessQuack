using System.Collections.Generic;
using InstagramApiSharp.Classes.Models;
using Quarkless.Base.Messaging.Models.Interfaces;

namespace Quarkless.Base.Messaging.Models
{
	public class ShareDirectMediaModel : IDirectMessageModel
	{
		public IEnumerable<string> Recipients { get; set; }
		public string MediaId { get; set; }
		public InstaMediaType MediaType { get; set; }
		public string Text { get; set; }
		public IEnumerable<string> ThreadIds { get; set; }
	}
}