using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Quarkless.Models.Profile
{
	public class AdditionalConfigurations
	{
		public bool IsTumblry { get; set; }
		public List<string> Sites { get; set; }
		public int ImageType { get; set; }
		public string PostSize { get; set; }
		public List<int> SearchTypes {get; set; }
		public string DefaultCaption { get; set; }
		public bool EnableOnlyAutoRepostFromUserTargetList { get; set; }
		public bool EnableAutoPosting { get; set; }
		public bool EnableAutoComment { get; set; }
		public bool EnableAutoLikeComment { get; set; }
		public bool EnableAutoLikePost { get; set; }
		public bool EnableAutoFollow { get; set; }
		public bool EnableAutoWatchStories { get; set; }
		public bool EnableAutoReactStories { get; set; }
		public bool EnableAutoDirectMessaging { get; set; }
		public bool AllowRepost { get; set; }
		public bool AutoGenerateCaption { get; set; }
		public bool FocusLocalMore { get; set; }

		[BsonRepresentation(BsonType.DateTime)]
		public DateTime WakeTime { get; set; }

		[BsonRepresentation(BsonType.DateTime)]
		public DateTime SleepTime { get; set; }

		public AdditionalConfigurations()
		{
			Sites = new List<string>();
			SearchTypes = new List<int>();
		}
	}
}