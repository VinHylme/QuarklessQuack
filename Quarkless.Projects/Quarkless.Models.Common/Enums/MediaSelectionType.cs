using System.ComponentModel;

namespace Quarkless.Models.Common.Enums
{
	public enum MediaSelectionType
	{
		[Description("image")]
		Image = 1,
		[Description("video")]
		Video = 2,
		[Description("carousel")]
		Carousel = 3
	}
}