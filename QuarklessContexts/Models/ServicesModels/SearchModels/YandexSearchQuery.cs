using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace QuarklessContexts.Models.ServicesModels.SearchModels
{
	public enum Orientation
	{
		Any,
		[Description("horizontal")]
		Landscape,
		[Description("vertical")]
		Portrait,
		[Description("square")]
		Square
	}
	public enum ImageType
	{
		Any,
		[Description("photo")]
		Photo,
		[Description("clipart")]
		WithWhiteBackground,
		[Description("lineart")]
		Drawinings,
		[Description("face")]
		People
	}
	public enum ColorType
	{
		[Description("any")]
		Any,
		[Description("color")]
		ColoredImagesOnly,
		[Description("gray")]
		BlackAndWhite,
		[Description("red")]
		Red,
		[Description("yellow")]
		Yellow,
		[Description("orange")]
		Orange,
		[Description("cyan")]
		Cyan,
		[Description("teal")]
		Teal,
		[Description("green")]
		Green,
		[Description("blue")]
		Blue,
		[Description("violet")]
		Violet,
		[Description("pink")]
		Pink,
		[Description("purple")]
		Purple,
		[Description("white")]
		White,
		[Description("black")]
		Black,
		[Description("brown")]
		Brown
	}
	public enum FormatType
	{
		Any,
		[Description("jpg")]
		JPEG,
		[Description("png")]
		PNG,
		[Description("gifan")]
		GIF
	}
	public enum SizeType
	{
		None,
		[Description("large")]
		Large,
		[Description("medium")]
		Medium,
		[Description("small")]
		Small
	}
	public struct SpecificSize
	{
		public int Width { get; set; }
		public int Height { get; set; }
	}
	public class YandexSearchQuery
	{
		public string OriginalTopic { get; set; }
		public string SearchQuery { get; set; }
		public Orientation Orientation { get; set; }
		public ImageType Type { get; set; }
		public ColorType Color { get; set; }
		public FormatType Format { get; set; }
		public SizeType Size { get; set; } = SizeType.None;
		public SpecificSize? SpecificSize { get; set; } = null;
	}
}
