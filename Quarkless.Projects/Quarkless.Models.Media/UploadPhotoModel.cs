using InstagramApiSharp.Classes.Models;
using Quarkless.Models.Common.Interfaces;
using Quarkless.Models.Topic;

namespace Quarkless.Models.Media
{
	public class UploadPhotoModel : IExec
	{
		public CTopic MediaTopic { get; set; }
		public InstaImageUpload Image { get; set; }
		public MediaInfo MediaInfo { get; set; }
		public InstaLocationShort Location { get; set; } = null;
	}
}