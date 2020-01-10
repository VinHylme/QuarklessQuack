using System.ComponentModel;

namespace QuarklessContexts.Models.Profiles
{
	public enum SearchType
	{
		[Description("Google")]
		Google = 0,
		[Description("Yandex")]
		Yandex = 1,
		[Description("Instagram")]
		Instagram = 2
	}
}