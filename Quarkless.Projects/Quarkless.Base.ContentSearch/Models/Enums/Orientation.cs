using System.ComponentModel;

namespace Quarkless.Base.ContentSearch.Models.Enums
{
	public enum Orientation
	{
		[Description("any")]
		Any,
		[Description("horizontal")]
		Landscape,
		[Description("vertical")]
		Portrait,
		[Description("square")]
		Square
	}
}
