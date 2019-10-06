﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Quarkless.Enums;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models.Account;
using Stripe;
using Stripe.Checkout;

namespace QuarklessLogic.Logic.AccountLogic
{
	public class AccountLogic : IAccountLogic
	{

		public AccountLogic()
		{
			StripeConfiguration.ApiKey = "sk_test_nWTSzRRrRHLlMWKBwKIh7hui00v6s2nMMh";
		}

		private int GetAmount(ChargeType premiumType)
		{
			switch (premiumType)
			{
				case ChargeType.AdditionalAccount:
					return 8;
				case ChargeType.Basic:
					return 30;
				case ChargeType.Premium:
					return 50;
				case ChargeType.Enterprise:
					return 100;
				default:
					throw new StripeException(HttpStatusCode.BadRequest, new StripeError
					{
						ErrorType = "validation_error"
					}, message: "Invalid option selected");
			}
		}

		public Session CreateSubscriptionSession(ChargeRequest chargeRequest)
		{
			try
			{
				var options = new SessionCreateOptions
				{
					PaymentMethodTypes = new List<string>
					{
						"card",
					},
					SubscriptionData = new SessionSubscriptionDataOptions
					{
						Items = new List<SessionSubscriptionDataItemOptions>
						{
							new SessionSubscriptionDataItemOptions
							{
								PlanId = chargeRequest.ChargeType.GetDescription(),
							},
						},
					},
					SuccessUrl = "https://example.com/success?session_id={CHECKOUT_SESSION_ID}",
					CancelUrl = "https://example.com/cancel",
				};

				var service = new SessionService();
				var session = service.Create(options);
				return session;
			}
			catch (StripeException e)
			{
				switch (e.StripeError.ErrorType)
				{
					case "card_error":
						break;
					case "api_connection_error":
						break;
					case "api_error":
						break;
					case "authentication_error":
						break;
					case "invalid_request_error":
						break;
					case "rate_limit_error":
						break;
					case "validation_error":
						break;
					default:
						// Unknown Error Type
						break;
				}
			}
			catch (Exception e)
			{

			}

			return null;
		}
		public Charge CreateCharge(ChargeRequest chargeRequest)
		{
			try 
			{
				var options = new ChargeCreateOptions()
				{
					Amount = GetAmount(chargeRequest.ChargeType),
					Currency = chargeRequest.Currency,
					Source = chargeRequest.Source,
					Metadata = new Dictionary<String, String>()
					{
						{ "OrderId", "6735"}
					}
				};

				var service = new ChargeService();
				var charge = service.Create(options);
				return charge;

			} catch (StripeException e) {
				switch (e.StripeError.ErrorType)
				{
					case "card_error":
						break;
					case "api_connection_error":
						break;
					case "api_error":
						break;
					case "authentication_error":
						break;
					case "invalid_request_error":
						break;
					case "rate_limit_error":
						break;
					case "validation_error":
						break;
					default:
						// Unknown Error Type
						break;
				}
			}

			return null;
		}

	}
}
