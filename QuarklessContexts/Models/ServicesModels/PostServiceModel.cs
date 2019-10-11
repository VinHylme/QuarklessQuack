using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace QuarklessContexts.Models.ServicesModels
{
	public class Image
	{
		public string Uri { get;set;}
		public int Width { get; set; }
		public int Height { get; set; }
		public byte[] ImageBytes { get; set; }
	}
	public class Video
	{
		public string Uri { get; set; }
		public byte[] VideoBytes { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
		public int Type { get; set; }
		public double Length { get; set; }
	}

	public class Media
	{
		public int Width { get; set; }
		public int Height { get; set; }
		public string Uri { get; set; }
	}
	public class Location
	{
		public long LocationId { get; set; }
		public string Name { get; set; }

	}
	public class Position
	{
		public double X { get; set; }
		public double Y { get; set; }
	}
	public class UserTag
	{
		public long User { get; set; }
		public Position PositionAt { get;set;}
	}

	public class PostServiceModel
	{
		[BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
		[BsonId]
		public string _id { get; set; }
		public string Topic { get; set; }
		public int LikeCount { get; set; }
		public int ViewsCount { get; set; }
		public DateTime CreatedDate { get; set; }
		public string MediaId { get; set; }
		public bool CanViewerReshare { get; set; }
		public Media Media { get; set; }
		public List<UserTag> UserTags { get; set; }
		public int MediaType { get; set; }   //0:image, 1:video, 2:carosoles
		public string TotalCommentCount { get; set; }
		public int UsedBeforeCount { get; set; }
		public Location Location { get; set; }
		
	}
}
