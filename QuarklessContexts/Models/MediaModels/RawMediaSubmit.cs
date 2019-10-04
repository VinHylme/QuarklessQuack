using System;
using System.Collections.Generic;
using QuarklessContexts.Models.Library;

namespace QuarklessContexts.Models.MediaModels
{
	public class RawMediaData
	{
		public string UrlToSend { get; set; }
		public string Media64BaseData { get; set; }
		public MediaSelectionType MediaType { get; set; }
	}
	public class RawMediaSubmit
	{
		public DateTime ExecuteTime { get; set;  }
		public string Caption { get; set; }
		public List<string> Hashtags { get; set; }
		public ShortLocation Location { get; set; }
		public MediaSelectionType OptionSelected { get; set; }
		public IEnumerable<RawMediaData> RawMediaDatas { get; set;  }
	}

	public class ShortLocation
	{
		public string Address { get; set;  }
		public string City { get; set;  }
	}
}
