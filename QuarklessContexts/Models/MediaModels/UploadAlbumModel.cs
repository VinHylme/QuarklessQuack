using InstagramApiSharp.Classes.Models;
using QuarklessContexts.Models.Topics;

namespace QuarklessContexts.Models.MediaModels
{
	public class UploadAlbumModel
	{
		public CTopic MediaTopic { get; set; }
		public InstaAlbumUpload[] Album {get;set; }
		public MediaInfo MediaInfo { get; set; }
		public InstaLocationShort Location { get; set; } = null;
	}
}
