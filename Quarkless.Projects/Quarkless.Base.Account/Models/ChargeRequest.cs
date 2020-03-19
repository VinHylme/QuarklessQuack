using Quarkless.Base.Account.Models.Enums;

namespace Quarkless.Base.Account.Models
{
	public class ChargeRequest
	{
		public ChargeType ChargeType { get; set; } = ChargeType.Basic;
		public string Currency { get; set; } = "gbp";
		public string Source { get; set; }
		public string AccountId { get; set; }
	}
}
