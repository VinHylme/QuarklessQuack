using InstagramApiSharp.Classes.Models;
using Quarkless.Models.Common.Models;

namespace Quarkless.Models.Services.Automation.Models.PostAction
{
	public struct MediaData
	{
		public Meta<SearchResponse.Media> SelectedMedia;
		public byte[] MediaBytes;
		public string Url { get; set; }
		public InstaMediaType MediaType { get; set; }
	}
}
