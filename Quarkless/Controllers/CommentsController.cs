using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using QuarklessContexts.Contexts;
using QuarklessContexts.Enums;
using QuarklessContexts.Models.Requests;
using QuarklessContexts.Models.UserAuth.AuthTypes;
using QuarklessLogic.Logic.CommentLogic;
using QuarklessLogic.Logic.ResponseLogic;
using QuarklessContexts.Extensions;
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
		private readonly IResponseResolver _responseResolver;
		public CommentsController(IUserContext userContext, ICommentLogic commentLogic, IResponseResolver responseResolver)
		{
			_commentLogic = commentLogic;
			_userContext = userContext;
			_responseResolver = responseResolver;
		}

		[HttpPost]
		[Route("api/comments/blockUser")]
		public async Task<IActionResult> BlockUserCommenting ([FromBody]List<long> userIds)
		{
			if (!_userContext.UserAccountExists || userIds == null) return BadRequest("invalid");
			var results = await _responseResolver.WithResolverAsync(
				await _commentLogic.BlockUserCommentingAsync(userIds.ToArray()), ActionType.None, userIds.ToJsonString());
			
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
			var results = await _responseResolver.WithResolverAsync(
				await _commentLogic.CommentMediaAsync(mediaId,comment.Text), ActionType.CreateCommentMedia, 
				JsonConvert.SerializeObject(new { MediaId = mediaId, Comment = comment }));
			if (results.Succeeded)
			{
				return Ok(results.Value);
			}
			return NotFound(results?.Info);
		}

		[HttpDelete]
		[Route("api/comments/delete/{mediaId}/{commentId}")]
		public async Task<IActionResult> DeleteComment([FromRoute]string mediaId, [FromRoute] string commentId)
		{
			if (!_userContext.UserAccountExists || string.IsNullOrEmpty(mediaId) || string.IsNullOrEmpty(commentId))
				return BadRequest("invalid");
			var results = await  _responseResolver.WithResolverAsync(
				await _commentLogic.DeleteCommentAsync(mediaId, commentId),ActionType.None, commentId);
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
			var results = await  _responseResolver.WithResolverAsync(
				await _commentLogic.DeleteMultipleCommentsAsync(mediaId, commentIds.ToArray()),ActionType.None,commentIds.ToJsonString());
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
			var results = await  _responseResolver.WithResolverAsync(
				await _commentLogic.DisableMediaCommentAsync(mediaId),ActionType.None, mediaId);
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
			var results = await  _responseResolver.WithResolverAsync(
				await _commentLogic.EnableMediaCommentAsync(mediaId),ActionType.None, mediaId);
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
			var results = await  _responseResolver.WithResolverAsync(
				await _commentLogic.GetBlockedCommentersAsync(),ActionType.None, "");
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
			var results = await  _responseResolver.WithResolverAsync(
				await _commentLogic.GetMediaCommentLikersAsync(mediaId), ActionType.None, mediaId);
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
			var results = await  _responseResolver.WithResolverAsync(
				await _commentLogic.GetMediaCommentsAsync(mediaId,limit),ActionType.None, mediaId);
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
			var results = await  _responseResolver.WithResolverAsync(
				await _commentLogic.GetMediaRepliesCommentsAsync(mediaId,targetCommentId, limit), ActionType.None, mediaId);
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
			var results = await  _responseResolver.WithResolverAsync(
				await _commentLogic.LikeCommentAsync(commentId),ActionType.LikeComment, commentId);
			if (results.Succeeded)
			{
				return Ok(results.Value);
			}
			return NotFound(results?.Info);
		}

		[HttpPost]
		[Route("api/comments/reply/{mediaId}/{targetCommentId}")]
		public async Task<IActionResult> ReplyCommentMedia([FromRoute]string mediaId, [FromRoute] string targetCommentId,[FromBody] string text)
		{
			if (!_userContext.UserAccountExists) return BadRequest("invalid");
			var results = await  _responseResolver.WithResolverAsync(
				await _commentLogic.ReplyCommentMediaAsync(mediaId, targetCommentId, text), ActionType.CreateCommentMedia, targetCommentId);
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
			var results = await  _responseResolver.WithResolverAsync(
				await _commentLogic.ReportCommentAsync(mediaId,commentId), ActionType.None, commentId);
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
			var results = await  _responseResolver.WithResolverAsync(
				await _commentLogic.TranslateCommentsAsync(commentIds.ToArray()), ActionType.None,commentIds.ToJsonString());
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
			var results = await _responseResolver.WithResolverAsync(
				await _commentLogic.UnblockUserCommentingAsync(userIds.ToArray()),ActionType.None, userIds.ToJsonString());
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
			var results = await  _responseResolver.WithResolverAsync(
				await _commentLogic.UnlikeCommentAsync(commentId), ActionType.UnlikeComment, commentId);
			if (results.Succeeded)
			{
				return Ok(results.Value);
			}
			return NotFound(results.Info);
		}
	}
}