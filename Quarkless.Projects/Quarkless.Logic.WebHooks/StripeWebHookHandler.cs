using Quarkless.Models.WebHooks.Interfaces;
using System;
using Stripe;

namespace Quarkless.Logic.WebHooks
{
	public class StripeWebHookHandler : IWebHookHandler
	{
		public void Handler(string json)
		{
			var stripeEvent = EventUtility.ParseEvent(json);

			switch (stripeEvent.Type)
			{

				case Events.AccountApplicationDeauthorized:
				case Events.AccountExternalAccountCreated:
				case Events.AccountExternalAccountDeleted:
				case Events.AccountExternalAccountUpdated:
				case Events.AccountUpdated:
					Console.WriteLine(stripeEvent.Data.Object.ToString());
					break;
				case Events.ApplicationFeeCreated:
				case Events.ApplicationFeeRefunded:
				case Events.ApplicationFeeRefundUpdated:
					Console.WriteLine(stripeEvent.Data.Object.ToString());
					break;
				case Events.BalanceAvailable:
					Console.WriteLine(stripeEvent.Data.Object.ToString());
					break;
				case Events.ChargeCaptured:
				case Events.ChargeFailed:
				case Events.ChargePending:
				case Events.ChargeRefunded:
				case Events.ChargeSucceeded:
				case Events.ChargeUpdated:
					Console.WriteLine(stripeEvent.Data.Object.ToString());
					break;
				case Events.ChargeDisputeClosed:
				case Events.ChargeDisputeCreated:
				case Events.ChargeDisputeFundsReinstated:
				case Events.ChargeDisputeFundsWithdrawn:
					Console.WriteLine(stripeEvent.Data.Object.ToString());
					break;
				case Events.CouponCreated:
				case Events.CouponDeleted:
				case Events.CouponUpdated:
					Console.WriteLine(stripeEvent.Data.Object.ToString());
					break;
				case Events.CustomerCreated:
				case Events.CustomerDeleted:
				case Events.CustomerUpdated:
					Console.WriteLine(stripeEvent.Data.Object.ToString());
					break;
				case Events.CustomerDiscountCreated:
				case Events.CustomerDiscountDeleted:
				case Events.CustomerDiscountUpdated:
					Console.WriteLine(stripeEvent.Data.Object.ToString());
					break;
				case Events.CustomerSourceCreated:
					Console.WriteLine(stripeEvent.Data.Object.ToString());
					break;
				case Events.CustomerSubscriptionCreated:
					break;
				case Events.InvoiceCreated:
					break;
				case Events.InvoicePaymentSucceeded:
					break;
				case Events.InvoicePaymentFailed:
					break;
				case Events.Ping:
					break;
				case Events.PlanCreated:
					break;
				case Events.PayoutCreated:
					break;
				case Events.PaymentCreated:
					break;
				case Events.PaymentIntentAmountCapturableUpdated:
					break;
				case Events.PaymentIntentCanceled:
					break;
				case Events.PaymentIntentCreated:
					break;
				case Events.PaymentIntentSucceeded:
					break;
				case Events.PayoutPaid:
					break;
				default:
					break;
			}
		}
	}
}
