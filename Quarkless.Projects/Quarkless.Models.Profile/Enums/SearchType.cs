﻿using System.ComponentModel;

namespace Quarkless.Models.Profile.Enums
{
	public enum SearchType
	{
		[Description("Google")]
		Google = 0,
		[Description("Yandex")]
		Yandex = 1,
		[Description("YandexQuery")]
		YandexQuery = 3,
		[Description("Instagram")]
		Instagram = 2
	}
}