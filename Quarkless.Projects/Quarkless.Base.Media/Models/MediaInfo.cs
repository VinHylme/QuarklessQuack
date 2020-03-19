using System.Collections.Generic;
using InstagramApiSharp.Classes.Models;

namespace Quarkless.Base.Media.Models
{
	public class MediaInfo
	{
		public InstaMediaType MediaType { get; set; }
		public string Caption { get; set; }
		public string Credit { get; set; }
		public List<string> Hashtags { get; set; }
	}
}