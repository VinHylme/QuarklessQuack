using InstagramApiSharp.Classes.Models;

namespace QuarklessContexts.Models.MediaModels
{
	public class UploadVideoModel
	{
		public InstaVideoUpload Video { get; set; }
		public MediaInfo MediaInfo { get; set; }
		public InstaLocationShort Location {get;set; } = null;
	}
}
