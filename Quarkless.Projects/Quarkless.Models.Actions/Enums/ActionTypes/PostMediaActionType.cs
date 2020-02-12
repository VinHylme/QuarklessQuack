using System.ComponentModel;

namespace Quarkless.Models.Actions.Enums.ActionTypes
{
	public enum PostMediaActionType
	{
		[Description("Any")]
		Any = 0,
		[Description("Image")]
		Image = 1,
		[Description("Video")]
		Video = 2,
		[Description("Carousel")]
		Carousel = 8
	}
}