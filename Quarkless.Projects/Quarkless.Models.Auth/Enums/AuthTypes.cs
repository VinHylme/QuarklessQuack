using System.ComponentModel;

namespace Quarkless.Models.Auth.Enums
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
