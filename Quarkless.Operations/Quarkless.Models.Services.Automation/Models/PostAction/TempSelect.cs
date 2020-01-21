using System.Collections.Generic;
using InstagramApiSharp.Classes.Models;

namespace Quarkless.Models.Services.Automation.Models.PostAction
{
	public class TempSelect
	{
		public InstaMediaType MediaType;
		public List<MediaData> MediaData = new List<MediaData>();
	}
}