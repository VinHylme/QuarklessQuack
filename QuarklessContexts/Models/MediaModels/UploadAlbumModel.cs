using InstagramApiSharp.Classes.Models;

namespace QuarklessContexts.Models.MediaModels
{
	public class UploadAlbumModel
	{
		public InstaAlbumUpload[] Album {get;set; }
		public MediaInfo MediaInfo { get; set; }
		public InstaLocationShort Location { get; set; } = null;
	}
}
