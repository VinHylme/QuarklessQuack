using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuarklessLogic.Handlers.WebHooks;

namespace Quarkless.Controllers
{
	public class HooksController : ControllerBase
	{
		private readonly IWebHookHandlers _hookHandlers;
	    public HooksController(IWebHookHandlers hookHandlers)
	    {
		    _hookHandlers = hookHandlers;
	    }

	    [HttpPost]
	    [Route("y1/hooks/stripe")]
	    public async Task<IActionResult> StripeHooks()
	    {
		    try
		    {
			    var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
				_hookHandlers.StripeHandler(json);
			    return Ok("Acknowledged");
		    }
		    catch (Exception ee)
		    {
			    return BadRequest(ee.Message);
		    }
	    }

    }
}