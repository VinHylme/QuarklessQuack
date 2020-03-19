using Stripe.Checkout;

namespace Quarkless.Base.Account.Models.Interfaces
{
	public interface IAccountLogic
	{
		Session CreateSubscriptionSession(ChargeRequest chargeRequest);
	}
}