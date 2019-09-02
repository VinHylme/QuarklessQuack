using System.Collections.Generic;
using InstagramApiSharp.Classes.Models;

namespace QuarklessContexts.Models.MessagingModels
{
	public class SendDirectTextModel
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
	public class SendDirectLinkModel
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
	public class SendDirectPhotoModel
	{
		public IEnumerable<string> Recipients { get; set; }
		public InstaImage Image { get; set;  }
		public SendDirectPhotoModel()
		{
			Recipients = new List<string>();
		}
	}
	public class SendDirectVideoModel
	{
		public IEnumerable<string> Recipients { get; set; }
		public InstaVideoUpload Video { get; set;  }
		public SendDirectVideoModel()
		{
			Recipients = new List<string>();
		}
	}
	public class SendDirectProfileModel
	{
		public IEnumerable<string> Recipients { get; set; }
		public long userId { get; set;  }
		public SendDirectProfileModel()
		{
			Recipients = new List<string>();
		}
	}
}
