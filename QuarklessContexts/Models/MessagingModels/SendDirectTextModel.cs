using System;
using System.Collections.Generic;
using InstagramApiSharp.Classes.Models;

namespace QuarklessContexts.Models.MessagingModels
{
	public interface IDirectMessageModel
	{
		IEnumerable<string> Recipients { get; set; }
	}
	public class TimelineRequestCarrier<TRequest>
	{
		public DateTime ExecuteTime { get; set;  }
		public TRequest Request { get; set;}
	}

	public class TimelineScheduleResponse<T>
	{
		public T RequestData { get; set; }
		public int NumberOfFails { get; set; }
		public bool IsSuccessful { get; set; } = false;
	}
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
	public class SendDirectLinkModel : IDirectMessageModel
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

	public class ShareDirectMediaModel : IDirectMessageModel
	{
		public IEnumerable<string> Recipients { get; set; }
		public string MediaId { get; set; }
		public InstaMediaType MediaType { get; set; }
		public string Text { get; set; }
		public IEnumerable<string> ThreadIds { get; set; }
	}

	public class SendDirectPhotoModel : IDirectMessageModel
	{
		public IEnumerable<string> Recipients { get; set; }
		public InstaImage Image { get; set;  }
		public SendDirectPhotoModel()
		{
			Recipients = new List<string>();
		}
	}
	public class SendDirectVideoModel : IDirectMessageModel
	{
		public IEnumerable<string> Recipients { get; set; }
		public InstaVideoUpload Video { get; set;  }
		public SendDirectVideoModel()
		{
			Recipients = new List<string>();
		}
	}
	public class SendDirectProfileModel : IDirectMessageModel
	{
		public IEnumerable<string> Recipients { get; set; }
		public long userId { get; set;  }
		public SendDirectProfileModel()
		{
			Recipients = new List<string>();
		}
	}
}
