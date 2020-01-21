using System.ComponentModel;

namespace Quarkless.Models.Shared.Enums
{
	public enum EnvironmentType
	{
		[Description("err")]
		None,
		[Description("local")]
		Local,
		[Description("dev")]
		Development,
		[Description("prod")]
		Production
	}
}
