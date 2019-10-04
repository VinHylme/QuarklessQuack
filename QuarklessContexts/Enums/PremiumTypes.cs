using System.ComponentModel;

namespace Quarkless.Enums
{
	public enum PremiumTypes
	{
		Trail,
		Basic,
		Premium,
		Enterprise,
	}

	public enum ChargeType
	{
		[Description("a01e7fcb")]
		AdditionalAccount,
		[Description("273d11a4")]
		Basic,
		[Description("a7c55c69")]
		Premium,
		[Description("c865717e")]
		Enterprise
	}
}
