using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using QuarklessContexts.Contexts;
using QuarklessContexts.Enums;
using QuarklessContexts.Models.UserAuth.AuthTypes;
using QuarklessLogic.Logic.BusinessLogic;
using QuarklessLogic.Logic.ResponseLogic;

namespace Quarkless.Controllers
{
    [ApiController]
    [HashtagAuthorize(AuthTypes.EnterpriseUsers)]
    [HashtagAuthorize(AuthTypes.TrialUsers)]
    [HashtagAuthorize(AuthTypes.PremiumUsers)]
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
			    .WithResolverAsync(await _businessLogic.GetStatisticsAsync(), ActionType.None, "");
		    if (results == null) return BadRequest("Invalid Request");
		    if (results.Succeeded)
		    {
			    return Ok(results.Value);
		    }

		    return BadRequest(results.Info);
	    }
    }
}