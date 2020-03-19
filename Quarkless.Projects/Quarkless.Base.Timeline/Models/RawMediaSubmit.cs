using System;
using System.Collections.Generic;
using Quarkless.Models.Common.Enums;

namespace Quarkless.Base.Timeline.Models
{
	public class RawMediaSubmit
	{
		public DateTime ExecuteTime { get; set; }
		public string Caption { get; set; }
		public List<string> Hashtags { get; set; }
		public ShortLocation Location { get; set; }
		public MediaSelectionType OptionSelected { get; set; }
		public IEnumerable<RawMediaData> RawMediaDatas { get; set; }
	}
}