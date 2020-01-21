using InstagramApiSharp.Classes.Models;

namespace Quarkless.Models.Media
{
	public class EditMediaModel
	{
		public string Caption { get; set; }
		public InstaLocationShort Location { get; set; } = null;
		public InstaUserTagUpload[] UserTags { get; set; } = null;
	}
}
