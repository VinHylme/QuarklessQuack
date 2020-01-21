using InstagramApiSharp.Classes.Models;
using Quarkless.Models.Topic;

namespace Quarkless.Models.Media
{
	public class UploadAlbumModel
	{
		public CTopic MediaTopic { get; set; }
		public InstaAlbumUpload[] Album { get; set; }
		public MediaInfo MediaInfo { get; set; }
		public InstaLocationShort Location { get; set; } = null;
	}
}