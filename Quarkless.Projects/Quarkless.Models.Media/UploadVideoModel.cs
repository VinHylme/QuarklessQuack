using InstagramApiSharp.Classes.Models;
using Quarkless.Models.Topic;

namespace Quarkless.Models.Media
{
	public class UploadVideoModel
	{
		public CTopic MediaTopic { get; set; }
		public InstaVideoUpload Video { get; set; }
		public MediaInfo MediaInfo { get; set; }
		public InstaLocationShort Location { get; set; } = null;
	}
}