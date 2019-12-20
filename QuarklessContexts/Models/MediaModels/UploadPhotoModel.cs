using InstagramApiSharp.Classes.Models;
using QuarklessContexts.Models.Topics;

namespace QuarklessContexts.Models.MediaModels
{
	public class UploadPhotoModel
	{
		public CTopic MediaTopic { get; set; }
		public InstaImageUpload Image {get; set; }
		public MediaInfo MediaInfo { get; set; }
		public InstaLocationShort Location { get; set; } = null;
	}
}
