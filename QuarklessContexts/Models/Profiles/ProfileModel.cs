using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace QuarklessContexts.Models.Profiles
{
	public class Color
	{
		public string Name { get; set; }
		public int Red { get; set; }
		public int Blue { get; set; }
		public int Green { get; set; }
		public int Alpha { get; set; }
	}
	public class GroupImagesAlike
	{
		public string TopicGroup { get; set; }
		public string Url { get; set; }
	}
	public class Themes
	{
		public string Name { get; set; }
		public List<Color> Colors { get; set; }
		public List<GroupImagesAlike> ImagesLike { get; set; }
		public double Percentage { get; set; }
	}
	public class Coordinates
	{
		public double Longitude { get; set; }
		public double Latitude { get; set; }
	}
	public class Location
	{
		public string City { get; set; }
		public string Address { get; set; }
		public string PostCode { get; set; }
		public Coordinates Coordinates { get; set; }
	}
	public enum SearchType
	{
		Google = 0,
		Yandex = 1,
		Instagram = 2
	}
	public class AdditionalConfigurations
	{
		public bool IsTumblry { get; set; }
		public List<string> Sites { get; set; }
		public string PostSize { get; set; }
		public List<int> SearchTypes {get; set; } = new List<int>();
		public AdditionalConfigurations()
		{
			Sites = new List<string>
			{
				"weheartit.com",
				"tumblr.com",
				"pinterest.co.uk"
			};
		}
	}
	public class ProfileModel
	{
		[BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
		[BsonId]
		public string _id { get; set; }
		public string Account_Id { get; set; }
		[BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
		public string InstagramAccountId { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public string Language { get; set; }
		public List<string> TopicList { get; set; }
		public List<string> UserTargetList { get; set; }
		public List<Location> LocationTargetList { get; set; }
		public AdditionalConfigurations AdditionalConfigurations { get; set; }
		public Themes Theme { get; set; }
	}
}
