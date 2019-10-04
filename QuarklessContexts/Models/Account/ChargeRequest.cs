using Quarkless.Enums;

namespace QuarklessContexts.Models.Account
{
	public class ChargeRequest
	{
		public ChargeType SelectedPremiumType { get; set; }
		public string Currency { get; set; } = "gbp";
		public string Source { get; set; }
		public string AccountId { get; set; }

	}
}
