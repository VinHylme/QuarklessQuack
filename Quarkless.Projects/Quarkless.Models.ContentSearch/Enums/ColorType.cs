using System.ComponentModel;

namespace Quarkless.Models.ContentSearch.Enums
{
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
}