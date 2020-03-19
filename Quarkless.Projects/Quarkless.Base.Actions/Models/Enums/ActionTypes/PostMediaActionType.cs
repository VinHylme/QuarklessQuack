using System.ComponentModel;

namespace Quarkless.Base.Actions.Models.Enums.ActionTypes
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