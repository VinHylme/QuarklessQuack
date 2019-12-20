using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace QuarklessContexts.Models.ServicesModels.SearchModels
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
