using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Quarkless.Base.InstagramBusiness;
using Quarkless.Models.Auth.Enums;
using Quarkless.Models.Auth.Interfaces;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.ResponseResolver.Interfaces;

namespace Quarkless.Controllers
{
    [ApiController]
    [HashtagAuthorize(AuthTypes.EnterpriseUsers)]
    [HashtagAuthorize(AuthTypes.TrialUsers)]
    [HashtagAuthorize(AuthTypes.PremiumUsers)]
    [HashtagAuthorize(AuthTypes.BasicUsers)]
	[HashtagAuthorize(AuthTypes.Admin)]
    public class BusinessController : ControllerBase
    {
	    private readonly IUserContext _userContext;
	    private readonly IResponseResolver _responseResolver;
	    private readonly IBusinessLogic _businessLogic;
	   
	    public BusinessController(IUserContext userContext, IResponseResolver responseResolver, IBusinessLogic businessLogic)
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
			    .WithResolverAsync(()=> _businessLogic.GetStatisticsAsync(), ActionType.None, "");
		    if (results == null) return BadRequest("Invalid Request");
		    if (results.Succeeded)
		    {
			    return Ok(results.Value);
		    }

		    return BadRequest(results.Info);
	    }
    }
}