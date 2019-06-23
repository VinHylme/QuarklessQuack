using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuarklessContexts.Models.ServicesModels.SearchModels
{
	public class MediaResponse
	{
		public string Topic { get; set; }
		public List<string> MediaUrl { get; set; } = new List<string>();
	}
	public class Media
	{
		public List<MediaResponse> Medias { get; set; }
		public int errors { get; set; }
		public Media()
		{
			Medias= new List<MediaResponse>();
		}
	}
}
