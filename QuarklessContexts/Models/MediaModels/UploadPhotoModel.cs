using InstagramApiSharp.Classes.Models;

namespace QuarklessContexts.Models.MediaModels
{
	public class UploadPhotoModel
	{
		public InstaImageUpload Image {get; set; }
		public MediaInfo MediaInfo { get; set; }
		public InstaLocationShort Location { get; set; } = null;
	}
}
