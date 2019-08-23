using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using QuarklessContexts.Contexts;
using QuarklessContexts.Enums;
using QuarklessContexts.Models.Requests;
using QuarklessContexts.Models.TimelineLoggingRepository;
using QuarklessContexts.Models.UserAuth.AuthTypes;
using QuarklessLogic.Logic.CommentLogic;
using QuarklessLogic.Logic.TimelineEventLogLogic;

namespace Quarkless.Controllers
{
    [ApiController]
	[HashtagAuthorize(AuthTypes.EnterpriseUsers)]
	[HashtagAuthorize(AuthTypes.TrialUsers)]
	[HashtagAuthorize(AuthTypes.BasicUsers)]
	[HashtagAuthorize(AuthTypes.PremiumUsers)]
	[HashtagAuthorize(AuthTypes.Admin)]
	public class CommentsController : ControllerBase
    {
		private readonly ICommentLogic _commentLogic;
		private readonly IUserContext _userContext;
		private readonly ITimelineEventLogLogic _timelineEventLogLogic;

		public CommentsController(IUserContext userContext, ICommentLogic commentLogic, ITimelineEventLogLogic timelineEventLogLogic)
		{
			_commentLogic = commentLogic;
			_userContext = userContext;
			_timelineEventLogLogic = timelineEventLogLogic;
		}

		[HttpPost]
		[Route("api/comments/blockUser")]
		public async Task<IActionResult> BlockUserCommenting ([FromBody]List<long> userIds)
		{
			if (!_userContext.UserAccountExists || userIds == null) return BadRequest("invalid");
			var results = await _commentLogic.BlockUserCommentingAsync(userIds.ToArray());
			if (results.Succeeded)
			{
				return Ok(results.Value);
			}
			return NotFound(results.Info);
		}
		[HttpPost]
		[Route("api/comments/create/{mediaId}")]
		public async Task<IActionResult> CommentMedia([FromRoute]string mediaId, [FromBody] CreateCommentRequest comment)
		{
			if (!_userContext.UserAccountExists || string.IsNullOrEmpty(comment.Text)) return BadRequest("invalid");
			var results = await _commentLogic.CommentMediaAsync(mediaId,comment.Text);
			if (results.Succeeded)
			{
				await _timelineEventLogLogic.AddTimelineLogFor(new TimelineEventLog
				{
					ActionType = ActionType.CreateCommentMedia,
					Message = $"Commented on {results.Value.User.UserName}'s Post ({mediaId}), Comment: {results.Value.Text} for: [{_userContext.CurrentUser}] Of [{_userContext.FocusInstaAccount}]",
					Request =  JsonConvert.SerializeObject(new { MediaId = mediaId, Comment = comment}),
					AccountID = _userContext.CurrentUser,
					InstagramAccountID = _userContext.FocusInstaAccount,
					Status = TimelineEventStatus.Success,
					Response =  JsonConvert.SerializeObject(results.Value),
					Level = 1
				});
				return Ok(results.Value);
			}
			await _timelineEventLogLogic.AddTimelineLogFor(new TimelineEventLog
			{
				ActionType = ActionType.CreateCommentMedia,
				Message = $"Failed to comment {results.Info.Message} for: [{_userContext.CurrentUser}] Of [{_userContext.FocusInstaAccount}]",
				Request =  JsonConvert.SerializeObject(new { MediaId = mediaId, Comment = comment}),
				AccountID = _userContext.CurrentUser,
				InstagramAccountID = _userContext.FocusInstaAccount,
				Status = TimelineEventStatus.Failed,
				Response =  JsonConvert.SerializeObject(results?.Info),
				Level = 2
			});
			return NotFound(results?.Info);
		}
		[HttpDelete]
		[Route("api/comments/delete/{mediaId}/{commentId}")]
		public async Task<IActionResult> DeleteComment([FromRoute]string mediaId, [FromRoute] string commentId)
		{
			if (!_userContext.UserAccountExists || string.IsNullOrEmpty(mediaId) || string.IsNullOrEmpty(commentId))
				return BadRequest("invalid");
			var results = await _commentLogic.DeleteCommentAsync(mediaId, commentId);
			if (results.Succeeded)
			{
				return Ok(results.Value);
			}
			return NotFound(results.Info);
		}
		[HttpDelete]
		[Route("api/comments/delete/{mediaId}")]
		public async Task<IActionResult> DeleteMultipleComments(string mediaId, [FromBody] List<string> commentIds)
		{
			if (!_userContext.UserAccountExists || commentIds == null || string.IsNullOrEmpty(mediaId))
				return BadRequest("invalid");
			var results = await _commentLogic.DeleteMultipleCommentsAsync(mediaId, commentIds.ToArray());
			if (results.Succeeded)
			{
				return Ok(results.Value);
			}
			return NotFound(results.Info);
		}
		[HttpPut]
		[Route("api/comments/disable/{mediaId}")]
		public async Task<IActionResult> DisableMediaComment([FromRoute]string mediaId)
		{
			if (!_userContext.UserAccountExists) return BadRequest("invalid");
			var results = await _commentLogic.DisableMediaCommentAsync(mediaId);
			if (results.Succeeded)
			{
				return Ok(results.Value);
			}
			return NotFound(results.Info);
		}
		[HttpPut]
		[Route("api/comments/enable/{mediaId}")]
		public async Task<IActionResult> EnableMediaComment([FromRoute]string mediaId)
		{
			if (!_userContext.UserAccountExists) return BadRequest("invalid");
			var results = await _commentLogic.EnableMediaCommentAsync(mediaId);
			if (results.Succeeded)
			{
				return Ok(results.Value);
			}
			return NotFound(results.Info);
		}
		[HttpGet]
		[Route("api/comments/blockedComments")]
		public async Task<IActionResult> GetBlockedCommenters()
		{
			if (!_userContext.UserAccountExists) return BadRequest("invalid");
			var results = await _commentLogic.GetBlockedCommentersAsync();
			if (results.Succeeded)
			{
				return Ok(results.Value);
			}
			return NotFound(results.Info);
		}
		[HttpGet]
		[Route("api/comments/mediaLikers/{mediaId}")]
		public async Task<IActionResult> GetMediaCommentLikers(string mediaId)
		{
			if (!_userContext.UserAccountExists) return BadRequest("invalid");
			var results = await _commentLogic.GetMediaCommentLikersAsync(mediaId);
			if (results.Succeeded)
			{
				return Ok(results.Value);
			}
			return NotFound(results.Info);
		}
		[HttpGet]
		[Route("api/comments/media/{mediaId}/{limit}")]
		public async Task<IActionResult> GetMediaComments(string mediaId, int limit=3)
		{
			if (!_userContext.UserAccountExists) return BadRequest("invalid");
			var results = await _commentLogic.GetMediaCommentsAsync(mediaId,limit);
			if (results.Succeeded)
			{
				return Ok(results.Value);
			}
			return NotFound(results.Info);
		}
		[HttpGet]
		[Route("api/comments/mediaReplies/{userIds}")]
		public async Task<IActionResult> GetMediaRepliesComments(string mediaId, string targetCommentId, int limit=3)
		{
			if (!_userContext.UserAccountExists) return BadRequest("invalid");
			var results = await _commentLogic.GetMediaRepliesCommentsAsync(mediaId,targetCommentId, limit);
			if (results.Succeeded)
			{
				return Ok(results.Value);
			}
			return NotFound(results.Info);
		}
		[HttpPost]
		[Route("api/comments/like/{commentId}")]
		public async Task<IActionResult> LikeComment(string commentId)
		{
			if (!_userContext.UserAccountExists) return BadRequest("invalid");
			var results = await _commentLogic.LikeCommentAsync(commentId);
			if (results.Succeeded)
			{
				await _timelineEventLogLogic.AddTimelineLogFor(new TimelineEventLog
				{
					ActionType = ActionType.LikeComment,
					Message = $"Liked Comment ({commentId}) for: [{_userContext.CurrentUser}] Of [{_userContext.FocusInstaAccount}]",
					Request = commentId,
					AccountID = _userContext.CurrentUser,
					InstagramAccountID = _userContext.FocusInstaAccount,
					Status = TimelineEventStatus.Success,
					Response = JsonConvert.SerializeObject(results.Value),
					Level = 1
				});
				return Ok(results.Value);
			}
			await _timelineEventLogLogic.AddTimelineLogFor(new TimelineEventLog
			{
				ActionType = ActionType.LikeComment,
				Message = $"Failed to like Comment ({commentId}) for: [{_userContext.CurrentUser}] Of [{_userContext.FocusInstaAccount}]",
				Request = commentId,
				AccountID = _userContext.CurrentUser,
				InstagramAccountID = _userContext.FocusInstaAccount,
				Status = TimelineEventStatus.Failed,
				Response =  JsonConvert.SerializeObject(results?.Info),
				Level = 2
			});
			return NotFound(results?.Info);
		}
		[HttpPost]
		[Route("api/comments/reply/{mediaId}/{targetCommentId}")]
		public async Task<IActionResult> ReplyCommentMedia([FromRoute]string mediaId, [FromRoute] string targetCommentId,[FromBody] string text)
		{
			if (!_userContext.UserAccountExists) return BadRequest("invalid");
			var results = await _commentLogic.ReplyCommentMediaAsync(mediaId, targetCommentId, text);
			if (results.Succeeded)
			{
				return Ok(results.Value);
			}
			return NotFound(results.Info);
		}
		[HttpPut]
		[Route("api/comments/report/{mediaId}/{commentId}")]
		public async Task<IActionResult> ReportComment([FromRoute]string mediaId, [FromRoute] string commentId)
		{
			if (!_userContext.UserAccountExists) return BadRequest("invalid");
			var results = await _commentLogic.ReportCommentAsync(mediaId,commentId);
			if (results.Succeeded)
			{
				return Ok(results.Value);
			}
			return NotFound(results.Info);
		}
		[HttpPut]
		[Route("api/comments/translate/")]
		public async Task<IActionResult> TranslateComments([FromBody]List<long> commentIds)
		{
			if (!_userContext.UserAccountExists) return BadRequest("invalid");
			var results = await _commentLogic.TranslateCommentsAsync(commentIds.ToArray());
			if (results.Succeeded)
			{
				return Ok(results.Value);
			}
			return NotFound(results.Info);
		}
		[HttpPut]
		[Route("api/comments/unblock")]
		public async Task<IActionResult> UnblockUserCommenting([FromBody]List<long> userIds)
		{
			if (!_userContext.UserAccountExists) return BadRequest("invalid");
			var results = await _commentLogic.UnblockUserCommentingAsync(userIds.ToArray());
			if (results.Succeeded)
			{
				return Ok(results.Value);
			}
			return NotFound(results.Info);
		}
		[HttpPut]
		[Route("api/comments/unlike/{commentId}")]
		public async Task<IActionResult> UnlikeComment([FromRoute]string commentId)
		{
			if (!_userContext.UserAccountExists) return BadRequest("invalid");
			var results = await _commentLogic.UnlikeCommentAsync(commentId);
			if (results.Succeeded)
			{
				return Ok(results.Value);
			}
			return NotFound(results.Info);
		}
	}
}