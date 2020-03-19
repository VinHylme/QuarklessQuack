using InstagramApiSharp.Classes.Models;
using Quarkless.Models.Common.Models;

namespace Quarkless.Base.Actions.Models.Models
{
	public class MediaData
	{
		public Meta<Quarkless.Models.SearchResponse.Media> SelectedMedia;
		public byte[] MediaBytes;
		public string Url { get; set; }
		public InstaMediaType MediaType { get; set; }
		public bool IncorrectFormat { get; set; }
	}
}