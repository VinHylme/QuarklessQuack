using System.Collections.Generic;
using Quarkless.Models.Common.Models.Topic;

namespace Quarkless.Models.SearchResponse.Structs
{
	public struct TempMedia
	{
		public struct Medias
		{
			public CTopic Topic { get; set; }
			public string MediaUrl { get; set; }
		}
		public List<Medias> MediasObject;
		public int errors { get; set; }
	}
}
