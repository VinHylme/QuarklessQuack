using System.ComponentModel;

namespace QuarklessContexts.Models.UserAuth.AuthTypes
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
