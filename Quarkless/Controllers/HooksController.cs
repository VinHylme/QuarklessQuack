using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Quarkless.Logic.WebHooks;
using Quarkless.Models.WebHooks.Interfaces;

namespace Quarkless.Controllers
{
	public class HooksController : ControllerBase
	{
		private readonly IWebHookHandler _hookHandlers;
	    public HooksController()
	    {
		    _hookHandlers = new StripeWebHookHandler();
	    }

	    [HttpPost]
	    [Route("y1/hooks/stripe")]
	    public async Task<IActionResult> StripeHooks()
	    {
		    try
		    {
			    var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
				_hookHandlers.Handler(json);
			    return Ok("Acknowledged");
		    }
		    catch (Exception ee)
		    {
			    return BadRequest(ee.Message);
		    }
	    }

    }
}