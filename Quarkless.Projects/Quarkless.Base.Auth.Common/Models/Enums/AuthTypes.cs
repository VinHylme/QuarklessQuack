using System.ComponentModel;

namespace Quarkless.Base.Auth.Common.Models.Enums
{
	public enum AuthTypes
	{
		[Description("Expired")]
		Expired,
		[Description("TrialUsers")]
		TrialUsers,
		[Description("BasicUsers")]
		BasicUsers,
		[Description("PremiumUsers")]
		PremiumUsers,
		[Description("EnterpriseUsers")]
		EnterpriseUsers,
		[Description("Admin")]
		Admin
	}
}
