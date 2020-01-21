using Quarkless.Models.Account.Enums;

namespace Quarkless.Models.Account
{
	public class ChargeRequest
	{
		public ChargeType ChargeType { get; set; } = ChargeType.Basic;
		public string Currency { get; set; } = "gbp";
		public string Source { get; set; }
		public string AccountId { get; set; }
	}
}
