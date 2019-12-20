using InstagramApiSharp.Classes.Models;
using QuarklessContexts.Models.Topics;

namespace QuarklessContexts.Models.MediaModels
{
	public class UploadVideoModel
	{
		public CTopic MediaTopic { get; set; }
		public InstaVideoUpload Video { get; set; }
		public MediaInfo MediaInfo { get; set; }
		public InstaLocationShort Location {get;set; } = null;
	}
}
