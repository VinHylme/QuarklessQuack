using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Quarkless.Models.SearchResponse
{
	[Serializable]
	public class Media
	{
		public List<MediaResponse> Medias { get; set; }
		[JsonProperty("errors")]
		public int Errors { get; set; }
		public Media()
		{
			Medias = new List<MediaResponse>();
		}
	}
}