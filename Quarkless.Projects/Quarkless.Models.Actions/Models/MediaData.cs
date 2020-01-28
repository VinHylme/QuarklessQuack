using InstagramApiSharp.Classes.Models;
using Quarkless.Models.Common.Models;

namespace Quarkless.Models.Actions.Models
{
	public class MediaData
	{
		public Meta<SearchResponse.Media> SelectedMedia;
		public byte[] MediaBytes;
		public string Url { get; set; }
		public InstaMediaType MediaType { get; set; }
		public bool IncorrectFormat { get; set; }
	}
}