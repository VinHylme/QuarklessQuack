﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Quarkless.Base.Auth.Common.Models.Enums;
using Quarkless.Base.Auth.Common.Models.Interfaces;
using Quarkless.Base.InstagramBusiness;
using Quarkless.Base.ResponseResolver.Models.Interfaces;

namespace Quarkless.Controllers
{
	[HashtagAuthorize(AuthTypes.EnterpriseUsers)]
	[HashtagAuthorize(AuthTypes.TrialUsers)]
	[HashtagAuthorize(AuthTypes.PremiumUsers)]
	[HashtagAuthorize(AuthTypes.BasicUsers)]
	[HashtagAuthorize(AuthTypes.Admin)]
	[ApiController]
	public class BusinessController : ControllerBaseExtended
	{
		private readonly IUserContext _userContext;
		private readonly IResponseResolver _responseResolver;
		private readonly IBusinessLogic _businessLogic;
		
		public BusinessController(IUserContext userContext, IResponseResolver responseResolver,
			IBusinessLogic businessLogic)
		{
			_userContext = userContext;
			_responseResolver = responseResolver;
			_businessLogic = businessLogic;
		}

		[HttpGet]
		[Route("api/analytical/stats")]
		public async Task<IActionResult> GetStatisticsAsync()
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid Request");

			var results = await _responseResolver
				.WithAttempts(1)
				.WithResolverAsync(()=> _businessLogic.GetStatisticsAsync());

			return ResolverResponse(results);
		}
	}
}