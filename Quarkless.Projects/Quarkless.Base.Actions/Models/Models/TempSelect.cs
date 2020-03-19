using System.Collections.Generic;
using InstagramApiSharp.Classes.Models;

namespace Quarkless.Base.Actions.Models.Models
{
	public class TempSelect
	{
		public InstaMediaType MediaType;
		public List<MediaData> MediaData = new List<MediaData>();
	}
}
