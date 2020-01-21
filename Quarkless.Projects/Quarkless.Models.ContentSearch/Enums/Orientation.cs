using System.ComponentModel;

namespace Quarkless.Models.ContentSearch.Enums
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
