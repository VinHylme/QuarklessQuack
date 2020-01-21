using InstagramApiSharp.Classes.Models;
using Quarkless.Models.Topic;

namespace Quarkless.Models.Media
{
	public class UploadPhotoModel
	{
		public CTopic MediaTopic { get; set; }
		public InstaImageUpload Image { get; set; }
		public MediaInfo MediaInfo { get; set; }
		public InstaLocationShort Location { get; set; } = null;
	}
}