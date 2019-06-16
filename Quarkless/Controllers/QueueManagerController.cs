using System;
using Microsoft.AspNetCore.Mvc;
using QuarklessContexts.Models.Events;
using QuarklessLogic.Logic.QueueLogic;

namespace Quarkless.Controllers
{
	public class QueueManagerController : ControllerBase
	{
		private readonly IQueueManagerLogic _queueManagerLogic;
		public QueueManagerController(IQueueManagerLogic queueManagerLogic)
		{
			_queueManagerLogic = queueManagerLogic;
		}

		[HttpPost]
		[Route("api/queue/add")]
		public IActionResult AddEvent([FromBody]EventQueueModel @event)
		{
			try
			{
				if (@event != null)
				{
					if (_queueManagerLogic.Enqueue(@event))
					{
						return Ok("Completed");
					}
					else
					{
						return BadRequest("Something went wrong");
					}

				}
				return BadRequest("Please do not pass null values");
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}

	}

}