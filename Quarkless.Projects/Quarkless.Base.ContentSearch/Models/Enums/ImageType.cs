using System.ComponentModel;

namespace Quarkless.Base.ContentSearch.Models.Enums
{
	public enum ImageType
	{
		[Description("any")]
		Any,
		[Description("photo")]
		Photo,
		[Description("clipart")]
		WithWhiteBackground,
		[Description("lineart")]
		Drawings,
		[Description("face")]
		People,
		[Description("animated")]
		Gif
	}
}