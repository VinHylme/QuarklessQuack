using QuarklessContexts.Models.Account;
using Stripe.Checkout;

namespace QuarklessLogic.Logic.AccountLogic
{
	public interface IAccountLogic
	{
		Session CreateSubscriptionSession(ChargeRequest chargeRequest);
	}
}