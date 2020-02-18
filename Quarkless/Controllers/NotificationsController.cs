using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Quarkless.Models.Auth.Enums;
using Quarkless.Models.Auth.Interfaces;
using Quarkless.Models.Notification.Enums;
using Quarkless.Models.Notification.Interfaces;

namespace Quarkless.Controllers
{
	[ApiController]
	[HashtagAuthorize(AuthTypes.EnterpriseUsers)]
	[HashtagAuthorize(AuthTypes.TrialUsers)]
	[HashtagAuthorize(AuthTypes.BasicUsers)]
	[HashtagAuthorize(AuthTypes.PremiumUsers)]
	[HashtagAuthorize(AuthTypes.Admin)]
	public class NotificationsController : ControllerBase
	{
		private readonly INotificationLogic _notificationLogic;
		private readonly IUserContext _userContext;
		public NotificationsController(INotificationLogic notificationLogic, IUserContext userContext)
		{
			_notificationLogic = notificationLogic;
			_userContext = userContext;
		}

		[HttpGet]
		[Route("api/notifications/actions/{instagramAccountId}/{limit}")]
		public async Task<IActionResult> GetActionNotifications(string instagramAccountId, int limit)
		{
			if (string.IsNullOrEmpty(_userContext.CurrentUser)) return BadRequest("Invalid Request");
			return Ok(await _notificationLogic
				.GetTimelineActionNotifications(_userContext.CurrentUser, instagramAccountId, limit,
					status:TimelineEventItemStatus.Success));
		}

		[HttpGet]
		[Route("api/notifications/actions/{limit}")]
		public async Task<IActionResult> GetTimelineEventLog(int limit)
		{
			if (string.IsNullOrEmpty(_userContext.CurrentUser)) return BadRequest("Invalid Request");
			if (string.IsNullOrEmpty(_userContext.FocusInstaAccount)) return BadRequest("Invalid user");
			return Ok(await _notificationLogic
				.GetTimelineActionNotifications(_userContext.CurrentUser, _userContext.FocusInstaAccount, limit,
					status: TimelineEventItemStatus.Success));
		}

		[HttpPost]
		[Route("api/notifications/mark-read")]
		public async Task<IActionResult> MarkNotificationAsRead(string[] notificationIds)
		{
			if (string.IsNullOrEmpty(_userContext.CurrentUser)) return BadRequest("Invalid Request");
			await _notificationLogic.MarkNotificationAsRead(notificationIds);
			return Ok("Marked Notifications as marked");
		}
	}
}
