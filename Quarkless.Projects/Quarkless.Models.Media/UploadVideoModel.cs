using InstagramApiSharp.Classes.Models;
using Quarkless.Models.Common.Interfaces;
using Quarkless.Models.Topic;

namespace Quarkless.Models.Media
{
	public class UploadVideoModel : IExec
	{
		public CTopic MediaTopic { get; set; }
		public InstaVideoUpload Video { get; set; }
		public MediaInfo MediaInfo { get; set; }
		public InstaLocationShort Location { get; set; } = null;
	}
}