using InstagramApiSharp.Classes.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace QuarklessContexts.Models.ServicesModels.SearchModels
{
	public enum MediaFrom
	{
		Instagram,
		Google,
		Yandex
	}

	[Serializable]
	public class MediaResponse
	{
		public string Topic { get; set; }
		public List<string> MediaUrl { get; set; } = new List<string>();
		public int LikesCount { get; set; }
		public int ViewCount { get; set; }
		public string CommentCount { get; set; }
		public string MediaId { get; set; }
		public bool HasLikedBefore { get; set; }
		public bool HasAudio { get; set; }
		public bool? IsFollowing { get; set; }
		public bool IsCommentsDisabled { get; set; }
		public InstaLocation Location { get; set; }
		public InstaMediaType MediaType { get; set; }
		public MediaFrom MediaFrom { get; set; }
		public UserResponse User { get; set; }
	}
	[Serializable]
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
