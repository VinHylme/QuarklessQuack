﻿using Quarkless.Base.ResponseResolver.Models.Enums;
using Quarkless.Models.Common.Extensions;

namespace Quarkless.Base.ResponseResolver.Models.Extensions
{
	public static class ChallengeResponseHandler
	{
		public static VerifyDataResponse GetVerifyResponse(this ChallengeHandleResponse handleResponse)
		{
			var verifyResponse = new VerifyDataResponse
			{
				Handled = false,
				StatusCode = VerifyStatusCode.Continue
			};

			switch (handleResponse.ResponseCode)
			{
				case ChallengeResponse.RequireEmailCode:
				case ChallengeResponse.RequireSmsCode:
				case ChallengeResponse.RequirePhoneNumber:
				case ChallengeResponse.Captcha:
					verifyResponse.VerifyMethod = handleResponse.VerifyMethod;
					verifyResponse.Verify = handleResponse.VerifyMethod.GetDescription();
					verifyResponse.VerifyDetail = handleResponse.StepDataEmailOrPhone;
					verifyResponse.ChallengeData = handleResponse.ChallengeData;
					return verifyResponse;
				case ChallengeResponse.Unknown:
				case ChallengeResponse.Ok:
				case ChallengeResponse.None:
					verifyResponse.Handled = true;
					verifyResponse.StatusCode = VerifyStatusCode.Ok;
					return verifyResponse;
				default:
					verifyResponse.StatusCode = VerifyStatusCode.Ok;
					return verifyResponse;
			}
		}

	}
}