using Stripe.Checkout;

namespace Quarkless.Models.Account.Interfaces
{
	public interface IAccountLogic
	{
		Session CreateSubscriptionSession(ChargeRequest chargeRequest);
	}
}