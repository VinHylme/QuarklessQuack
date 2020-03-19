using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Quarkless.Base.ResponseResolver.Models;
using Quarkless.Base.ResponseResolver.Models.Enums;
using Quarkless.Base.ResponseResolver.Models.Extensions;

namespace Quarkless.Controllers
{
	[DefaultStatusCode(DEFAULT_STATUS_CODE)]
	public class ChallengeResult : ObjectResult
	{
		private const int DEFAULT_STATUS_CODE = StatusCodes.Status201Created;
		public ChallengeResult(object value) : base(value)
		{
			StatusCode = DEFAULT_STATUS_CODE;
		}

		public ChallengeResult(int status, object value) : base(value)
		{
			StatusCode = status;
		}
	}

	[Controller]
	public abstract class ControllerBaseExtended : ControllerBase
	{
		[NonAction]
		public ChallengeResult OkChallengeResponse(ChallengeHandleResponse handleResponse)
		{
			var toResponse = handleResponse.GetVerifyResponse();
			return toResponse.StatusCode == VerifyStatusCode.Continue 
				? OkChallengeResult(toResponse) 
				: OkChallengeResult((int)toResponse.StatusCode, toResponse);
		}

		[NonAction]
		public ChallengeResult OkChallengeResult(object value)
			=> new ChallengeResult(value);

		[NonAction]
		public ChallengeResult OkChallengeResult(int status, object value)
			=> new ChallengeResult(status,value);

		public IActionResult ResolverResponse<TResult>(ResolverResponse<TResult> results,
			Func<bool> otherChecksForSuccess = null)
		{
			if (results == null)
				return BadRequest("Results came back null");

			if (otherChecksForSuccess == null)
				otherChecksForSuccess = () => true;

			if (results.Response.Succeeded && otherChecksForSuccess())
			{
				return Ok(results.Response.Value);
			}

			if (results.Response.Info.NeedsChallenge)
			{
				return OkChallengeResponse(results.HandlerResults.ChallengeResponse);
			}

			return NotFound(results.Response.Info);
		}
	}
}
