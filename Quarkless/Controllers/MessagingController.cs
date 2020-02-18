using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Quarkless.Models.Auth.Enums;
using Quarkless.Models.Auth.Interfaces;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Common.Extensions;
using Quarkless.Models.Messaging;
using Quarkless.Models.Messaging.Interfaces;
using Quarkless.Models.ResponseResolver.Interfaces;

namespace Quarkless.Controllers
{
	[ApiController]
	[HashtagAuthorize(AuthTypes.EnterpriseUsers)]
	[HashtagAuthorize(AuthTypes.TrialUsers)]
	[HashtagAuthorize(AuthTypes.PremiumUsers)]
	[HashtagAuthorize(AuthTypes.Admin)]
    public class MessagingController : ControllerBaseExtended
    {
	    private readonly IUserContext _userContext;
	    private readonly IResponseResolver _responseResolver;
	    private readonly IMessagingLogic _messagingLogic;
	    public MessagingController(IUserContext userContext, IResponseResolver responseResolver, IMessagingLogic messagingLogic)
	    {
		    _userContext = userContext;
		    _messagingLogic = messagingLogic;
		    _responseResolver = responseResolver;
	    }

	    [HttpGet]
	    [Route("api/messaging/inbox/{limit}")]
	    public async Task<IActionResult> GetInbox(int limit)
	    {
		    if (!_userContext.UserAccountExists) return BadRequest("Invalid Request");
		    var results = await _responseResolver.WithAttempts(1)
				.WithResolverAsync(()=> _messagingLogic.GetDirectInboxAsync(limit));
			return ResolverResponse(results);
		}

	    [HttpGet]
	    [Route("api/messaging/thread/{threadId}/{limit}")]
	    public async Task<IActionResult> GetThread(string threadId, int limit)
	    {
		    if (!_userContext.UserAccountExists) return BadRequest("Invalid Request");
		    var results = await _responseResolver.WithAttempts(1)
				.WithResolverAsync(()=> _messagingLogic.GetDirectInboxThreadAsync(threadId,limit));
			return ResolverResponse(results);
		}

	    [HttpPost]
	    [Route("api/messaging/text")]
		public async Task<IActionResult> SendDirectText([FromBody] SendDirectTextModel sendDirectText)
		{
		    if (!_userContext.UserAccountExists) return BadRequest("Invalid Request");
		    if (sendDirectText == null) return BadRequest("Invalid Params");

		    var recipients = string.Empty;
		    var threads = string.Empty;
			if(sendDirectText.Recipients!=null && sendDirectText.Recipients.Any()) 
				recipients = string.Join(",", sendDirectText.Recipients);
			if (sendDirectText.Threads != null && sendDirectText.Threads.Any())
				threads = string.Join(",", sendDirectText.Threads);

			var results = await _responseResolver.WithAttempts(1)
				.WithResolverAsync(()=>_messagingLogic.SendDirectTextAsync(recipients, threads, sendDirectText.TextMessage), 
					ActionType.SendDirectMessageText, sendDirectText);
			return ResolverResponse(results);
		}

		[HttpPost]
		[Route("api/messaging/link")]
		public async Task<IActionResult> SendDirectLink([FromBody] SendDirectLinkModel sendDirectLink)
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid Request");			
			if (sendDirectLink == null) return BadRequest("Invalid Params");
			var results = await _responseResolver
				.WithAttempts(1)
				.WithResolverAsync(()=> _messagingLogic.SendDirectLinkAsync(sendDirectLink.TextMessage, sendDirectLink.Link,
						sendDirectLink.Threads.ToArray(), sendDirectLink.Recipients.ToArray()), 
					ActionType.SendDirectMessageLink,
					sendDirectLink);
			return ResolverResponse(results);
		}

		[HttpPost]
		[Route("api/messaging/photo")]
		public async Task<IActionResult> SendDirectPhoto([FromBody] SendDirectPhotoModel sendDirectPhoto)
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid Request");
			if (sendDirectPhoto == null) return BadRequest("Invalid Params");
			var results = await _responseResolver
				.WithAttempts(1)
				.WithResolverAsync(()=>_messagingLogic.SendDirectPhotoToRecipientsAsync(sendDirectPhoto.Image, sendDirectPhoto.Recipients.ToArray()), 
					ActionType.SendDirectMessagePhoto,
					sendDirectPhoto);
			return ResolverResponse(results);
		}

		[HttpPost]
		[Route("api/messaging/media")]
		public async Task<IActionResult> ShareDirectMedia([FromBody] ShareDirectMediaModel shareDirectMedia)
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid Request");
			if (shareDirectMedia == null) return BadRequest("Invalid Params");
			var results = await _responseResolver
				.WithAttempts(1)
				.WithResolverAsync(()=> _messagingLogic.ShareMediaToUserAsync(shareDirectMedia.MediaId, shareDirectMedia.MediaType, shareDirectMedia.Text, shareDirectMedia.Recipients.Select(long.Parse).ToArray()), 
					ActionType.SendDirectMessageMedia,
					shareDirectMedia);
			return ResolverResponse(results);
		}

		[HttpPost]
		[Route("api/messaging/media-threads")]
		public async Task<IActionResult> ShareDirectMediaWithThreads([FromBody] ShareDirectMediaModel shareDirectMedia)
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid Request");
			if (shareDirectMedia == null) return BadRequest("Invalid Params");
			var results = await _responseResolver
				.WithAttempts(1)
				.WithResolverAsync(()=>_messagingLogic.ShareMediaToThreadAsync(shareDirectMedia.MediaId, shareDirectMedia.MediaType, shareDirectMedia.Text, shareDirectMedia.ThreadIds.ToArray()), 
					ActionType.SendDirectMessageMedia,
					shareDirectMedia);
			return ResolverResponse(results);
		}

		[HttpPost]
		[Route("api/messaging/video")]
		public async Task<IActionResult> SendDirectVideo([FromBody] SendDirectVideoModel sendDirectVideo)
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid Request");
			if (sendDirectVideo == null) return BadRequest("Invalid Params");
			var results = await _responseResolver
				.WithAttempts(1)
				.WithResolverAsync(()=>_messagingLogic.SendDirectVideoToRecipientsAsync(sendDirectVideo.Video, sendDirectVideo.Recipients.ToArray()), 
					ActionType.SendDirectMessageVideo,
					sendDirectVideo);
			return ResolverResponse(results);
		}

		[HttpPost]
		[Route("api/messaging/profile")]
		public async Task<IActionResult> SendProfile([FromBody] SendDirectProfileModel sendDirectProfile)
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid Request");
			if (sendDirectProfile == null) return BadRequest("Invalid Params");
			if (sendDirectProfile.userId==0) return BadRequest("Invalid Params");
			var recipients = string.Empty;
			if (sendDirectProfile.Recipients != null && sendDirectProfile.Recipients.Any())
				recipients = string.Join(",", sendDirectProfile.Recipients);

			var results = await _responseResolver
				.WithResolverAsync(()=> _messagingLogic.SendDirectProfileToRecipientsAsync(sendDirectProfile.userId, recipients), 
					ActionType.SendDirectMessageProfile,
					sendDirectProfile);
			return ResolverResponse(results);
		}
    }
}