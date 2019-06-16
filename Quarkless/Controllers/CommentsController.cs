using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Quarkless.Auth.AuthTypes;
using QuarklessContexts.Contexts;
using QuarklessContexts.Models.Requests;
using QuarklessLogic.Logic.CommentLogic;

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
		public CommentsController(IUserContext userContext, ICommentLogic commentLogic)
		{
			_commentLogic = commentLogic;
			_userContext = userContext;
		}

		[HttpPost]
		[Route("api/comments/blockUser")]
		public async Task<IActionResult> BlockUserCommenting ([FromBody]List<long> userIds)
		{
			if (_userContext.UserAccountExists && userIds!=null)
			{
				var results = await _commentLogic.BlockUserCommentingAsync(userIds.ToArray());
				if (results.Succeeded)
				{
					return Ok(results.Value);
				}
				return NotFound(results.Info);
			}
			return BadRequest("invalid");
		}
		[HttpPost]
		[Route("api/comments/create/{mediaId}")]
		public async Task<IActionResult> CommentMedia([FromRoute]string mediaId, [FromBody] CreateCommentRequest comment)
		{
			if (_userContext.UserAccountExists && !string.IsNullOrEmpty(comment.Text))
			{
				var results = await _commentLogic.CommentMediaAsync(mediaId,comment.Text);
				if (results.Succeeded)
				{
					return Ok(results.Value);
				}
				return NotFound(results.Info);
			}
			return BadRequest("invalid");
		}
		[HttpDelete]
		[Route("api/comments/delete/{mediaId}/{commentId}")]
		public async Task<IActionResult> DeleteComment([FromRoute]string mediaId, [FromRoute] string commentId)
		{
			if (_userContext.UserAccountExists && !string.IsNullOrEmpty(mediaId) && !string.IsNullOrEmpty(commentId))
			{
				var results = await _commentLogic.DeleteCommentAsync(mediaId, commentId);
				if (results.Succeeded)
				{
					return Ok(results.Value);
				}
				return NotFound(results.Info);
			}
			return BadRequest("invalid");
		}
		[HttpDelete]
		[Route("api/comments/delete/{mediaId}")]
		public async Task<IActionResult> DeleteMultipleComments(string mediaId, [FromBody] List<string> commentIds)
		{
			if (_userContext.UserAccountExists && commentIds!=null && !string.IsNullOrEmpty(mediaId))
			{
				var results = await _commentLogic.DeleteMultipleCommentsAsync(mediaId, commentIds.ToArray());
				if (results.Succeeded)
				{
					return Ok(results.Value);
				}
				return NotFound(results.Info);
			}
			return BadRequest("invalid");
		}
		[HttpPut]
		[Route("api/comments/disable/{mediaId}")]
		public async Task<IActionResult> DisableMediaComment([FromRoute]string mediaId)
		{
			if (_userContext.UserAccountExists)
			{
				var results = await _commentLogic.DisableMediaCommentAsync(mediaId);
				if (results.Succeeded)
				{
					return Ok(results.Value);
				}
				return NotFound(results.Info);
			}
			return BadRequest("invalid");
		}
		[HttpPut]
		[Route("api/comments/enable/{mediaId}")]
		public async Task<IActionResult> EnableMediaComment([FromRoute]string mediaId)
		{
			if (_userContext.UserAccountExists)
			{
				var results = await _commentLogic.EnableMediaCommentAsync(mediaId);
				if (results.Succeeded)
				{
					return Ok(results.Value);
				}
				return NotFound(results.Info);
			}
			return BadRequest("invalid");
		}
		[HttpGet]
		[Route("api/comments/blockedComments")]
		public async Task<IActionResult> GetBlockedCommenters()
		{
			if (_userContext.UserAccountExists)
			{
				var results = await _commentLogic.GetBlockedCommentersAsync();
				if (results.Succeeded)
				{
					return Ok(results.Value);
				}
				return NotFound(results.Info);
			}
			return BadRequest("invalid");
		}
		[HttpGet]
		[Route("api/comments/mediaLikers/{mediaId}")]
		public async Task<IActionResult> GetMediaCommentLikers(string mediaId)
		{
			if (_userContext.UserAccountExists)
			{
				var results = await _commentLogic.GetMediaCommentLikersAsync(mediaId);
				if (results.Succeeded)
				{
					return Ok(results.Value);
				}
				return NotFound(results.Info);
			}
			return BadRequest("invalid");
		}
		[HttpGet]
		[Route("api/comments/media/{mediaId}/{limit}")]
		public async Task<IActionResult> GetMediaComments(string mediaId, int limit=3)
		{
			if (_userContext.UserAccountExists)
			{
				var results = await _commentLogic.GetMediaCommentsAsync(mediaId,limit);
				if (results.Succeeded)
				{
					return Ok(results.Value);
				}
				return NotFound(results.Info);
			}
			return BadRequest("invalid");
		}
		[HttpGet]
		[Route("api/comments/mediaReplies/{userIds}")]
		public async Task<IActionResult> GetMediaRepliesComments(string mediaId, string targetCommentId, int limit=3)
		{
			if (_userContext.UserAccountExists)
			{
				var results = await _commentLogic.GetMediaRepliesCommentsAsync(mediaId,targetCommentId, limit);
				if (results.Succeeded)
				{
					return Ok(results.Value);
				}
				return NotFound(results.Info);
			}
			return BadRequest("invalid");
		}
		[HttpGet]
		[Route("api/comments/like/{commentId}")]
		public async Task<IActionResult> LikeComment(string commentId)
		{
			if (_userContext.UserAccountExists)
			{
				var results = await _commentLogic.LikeCommentAsync(commentId);
				if (results.Succeeded)
				{
					return Ok(results.Value);
				}
				return NotFound(results.Info);
			}
			return BadRequest("invalid");
		}
		[HttpPost]
		[Route("api/comments/reply/{mediaId}/{targetCommentId}")]
		public async Task<IActionResult> ReplyCommentMedia([FromRoute]string mediaId, [FromRoute] string targetCommentId,[FromBody] string text)
		{
			if (_userContext.UserAccountExists)
			{
				var results = await _commentLogic.ReplyCommentMediaAsync(mediaId, targetCommentId, text);
				if (results.Succeeded)
				{
					return Ok(results.Value);
				}
				return NotFound(results.Info);
			}
			return BadRequest("invalid");
		}
		[HttpPut]
		[Route("api/comments/report/{mediaId}/{commentId}")]
		public async Task<IActionResult> ReportComment([FromRoute]string mediaId, [FromRoute] string commentId)
		{
			if (_userContext.UserAccountExists)
			{
				var results = await _commentLogic.ReportCommentAsync(mediaId,commentId);
				if (results.Succeeded)
				{
					return Ok(results.Value);
				}
				return NotFound(results.Info);
			}
			return BadRequest("invalid");
		}
		[HttpPut]
		[Route("api/comments/translate/")]
		public async Task<IActionResult> TranslateComments([FromBody]List<long> commentIds)
		{
			if (_userContext.UserAccountExists)
			{
				var results = await _commentLogic.TranslateCommentsAsync(commentIds.ToArray());
				if (results.Succeeded)
				{
					return Ok(results.Value);
				}
				return NotFound(results.Info);
			}
			return BadRequest("invalid");
		}
		[HttpPut]
		[Route("api/comments/unblock")]
		public async Task<IActionResult> UnblockUserCommenting([FromBody]List<long> userIds)
		{
			if (_userContext.UserAccountExists)
			{
				var results = await _commentLogic.UnblockUserCommentingAsync(userIds.ToArray());
				if (results.Succeeded)
				{
					return Ok(results.Value);
				}
				return NotFound(results.Info);
			}
			return BadRequest("invalid");
		}
		[HttpPut]
		[Route("api/comments/unlike/{commentId}")]
		public async Task<IActionResult> UnlikeComment([FromRoute]string commentId)
		{
			if (_userContext.UserAccountExists)
			{
				var results = await _commentLogic.UnlikeCommentAsync(commentId);
				if (results.Succeeded)
				{
					return Ok(results.Value);
				}
				return NotFound(results.Info);
			}
			return BadRequest("invalid");
		}
	}
}