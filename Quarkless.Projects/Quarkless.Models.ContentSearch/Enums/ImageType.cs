﻿using System.ComponentModel;

namespace Quarkless.Models.ContentSearch.Enums
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
		Drawinings,
		[Description("face")]
		People
	}
}