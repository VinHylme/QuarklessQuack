using InstagramApiSharp.Classes.Models;

namespace Quarkless.Base.Media.Models
{
	public class EditMediaModel
	{
		public string Caption { get; set; }
		public InstaLocationShort Location { get; set; } = null;
		public InstaUserTagUpload[] UserTags { get; set; } = null;
	}
}
