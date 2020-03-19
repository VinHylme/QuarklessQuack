using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Quarkless.Base.Auth.Common.Models.Enums;
using Quarkless.Base.Auth.Common.Models.Interfaces;
using Quarkless.Base.InstagramComments.Models;
using Quarkless.Base.InstagramComments.Models.Interfaces;
using Quarkless.Base.ResponseResolver.Models.Interfaces;
using Quarkless.Models.Common.Enums;

namespace Quarkless.Controllers
{
    [ApiController]
	[HashtagAuthorize(AuthTypes.EnterpriseUsers)]
	[HashtagAuthorize(AuthTypes.TrialUsers)]
	[HashtagAuthorize(AuthTypes.BasicUsers)]
	[HashtagAuthorize(AuthTypes.PremiumUsers)]
	[HashtagAuthorize(AuthTypes.Admin)]
	public class CommentsController : ControllerBaseExtended
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
			var results = await _responseResolver.WithAttempts(1).WithResolverAsync(
				()=> _commentLogic.BlockUserCommentingAsync(userIds.ToArray()));

			return ResolverResponse(results);
		}

		[HttpPost]
		[Route("api/comments/create/{mediaId}")]
		public async Task<IActionResult> CommentMedia([FromRoute]string mediaId, [FromBody] CreateCommentRequest comment)
		{
			if (!_userContext.UserAccountExists || string.IsNullOrEmpty(comment.Text)) return BadRequest("invalid");
			var results = await _responseResolver.WithAttempts(1).WithResolverAsync(
				()=> _commentLogic.CommentMediaAsync(mediaId,comment.Text), ActionType.CreateCommentMedia, comment);

			return ResolverResponse(results);
		}

		[HttpDelete]
		[Route("api/comments/delete/{mediaId}/{commentId}")]
		public async Task<IActionResult> DeleteComment([FromRoute]string mediaId, [FromRoute] string commentId)
		{
			if (!_userContext.UserAccountExists || string.IsNullOrEmpty(mediaId) || string.IsNullOrEmpty(commentId))
				return BadRequest("invalid");
			var results = await  _responseResolver.WithAttempts(1).WithResolverAsync(
				()=> _commentLogic.DeleteCommentAsync(mediaId, commentId));

			return ResolverResponse(results);
		}

		[HttpDelete]
		[Route("api/comments/delete/{mediaId}")]
		public async Task<IActionResult> DeleteMultipleComments(string mediaId, [FromBody] List<string> commentIds)
		{
			if (!_userContext.UserAccountExists || commentIds == null || string.IsNullOrEmpty(mediaId))
				return BadRequest("invalid");
			var results = await  _responseResolver.WithAttempts(1).WithResolverAsync(
				()=> _commentLogic.DeleteMultipleCommentsAsync(mediaId, commentIds.ToArray()));
			
			return ResolverResponse(results);
		}

		[HttpPut]
		[Route("api/comments/disable/{mediaId}")]
		public async Task<IActionResult> DisableMediaComment([FromRoute]string mediaId)
		{
			if (!_userContext.UserAccountExists) return BadRequest("invalid");
			var results = await  _responseResolver.WithAttempts(1).WithResolverAsync(
				()=> _commentLogic.DisableMediaCommentAsync(mediaId));
			
			return ResolverResponse(results);
		}
		[HttpPut]
		[Route("api/comments/enable/{mediaId}")]
		public async Task<IActionResult> EnableMediaComment([FromRoute]string mediaId)
		{
			if (!_userContext.UserAccountExists) return BadRequest("invalid");
			var results = await  _responseResolver.WithAttempts(1).WithResolverAsync(
				()=> _commentLogic.EnableMediaCommentAsync(mediaId));
			
			return ResolverResponse(results);
		}

		[HttpGet]
		[Route("api/comments/blockedComments")]
		public async Task<IActionResult> GetBlockedCommenters()
		{
			if (!_userContext.UserAccountExists) return BadRequest("invalid");
			var results = await  _responseResolver.WithAttempts(1).WithResolverAsync(
				()=> _commentLogic.GetBlockedCommentersAsync());

			return ResolverResponse(results);
		}

		[HttpGet]
		[Route("api/comments/mediaLikers/{mediaId}")]
		public async Task<IActionResult> GetMediaCommentLikers(string mediaId)
		{
			if (!_userContext.UserAccountExists) return BadRequest("invalid");
			var results = await  _responseResolver.WithAttempts(1).WithResolverAsync(
				()=> _commentLogic.GetMediaCommentLikersAsync(mediaId));

			return ResolverResponse(results);
		}

		[HttpGet]
		[Route("api/comments/media/{mediaId}/{limit}")]
		public async Task<IActionResult> GetMediaComments(string mediaId, int limit=3)
		{
			if (!_userContext.UserAccountExists) return BadRequest("invalid");
			var results = await  _responseResolver.WithAttempts(1).WithResolverAsync(
				()=> _commentLogic.GetMediaCommentsAsync(mediaId,limit));
			
			return ResolverResponse(results);
		}

		[HttpGet]
		[Route("api/comments/mediaReplies/{userIds}")]
		public async Task<IActionResult> GetMediaRepliesComments(string mediaId, string targetCommentId, int limit=3)
		{
			if (!_userContext.UserAccountExists) return BadRequest("invalid");
			var results = await  _responseResolver.WithAttempts(1).WithResolverAsync(
				()=> _commentLogic.GetMediaRepliesCommentsAsync(mediaId,targetCommentId, limit));
			
			return ResolverResponse(results);
		}

		[HttpPost]
		[Route("api/comments/like/{commentId}")]
		public async Task<IActionResult> LikeComment(LikeCommentRequest comment)
		{
			if (!_userContext.UserAccountExists) return BadRequest("invalid");
			var results = await  _responseResolver.WithAttempts(1).WithResolverAsync(
				()=> _commentLogic.LikeCommentAsync(comment.CommentId.ToString()), ActionType.LikeComment, comment);
			
			return ResolverResponse(results);
		}

		[HttpPost]
		[Route("api/comments/reply/{mediaId}/{targetCommentId}")]
		public async Task<IActionResult> ReplyCommentMedia([FromRoute]string mediaId, [FromRoute] string targetCommentId, [FromBody] CreateCommentRequest text)
		{
			if (!_userContext.UserAccountExists) return BadRequest("invalid");
			var results = await  _responseResolver.WithAttempts(1).WithResolverAsync(
				()=> _commentLogic.ReplyCommentMediaAsync(mediaId, targetCommentId, text.Text), ActionType.CreateCommentMedia, text);
			
			return ResolverResponse(results);
		}
		
		[HttpPut]
		[Route("api/comments/report/{mediaId}/{commentId}")]
		public async Task<IActionResult> ReportComment([FromRoute]string mediaId, [FromRoute] string commentId)
		{
			if (!_userContext.UserAccountExists) return BadRequest("invalid");
			var results = await  _responseResolver.WithAttempts(1).WithResolverAsync(
				()=> _commentLogic.ReportCommentAsync(mediaId,commentId));
			return ResolverResponse(results);
		}
		
		[HttpPut]
		[Route("api/comments/translate/")]
		public async Task<IActionResult> TranslateComments([FromBody]List<long> commentIds)
		{
			if (!_userContext.UserAccountExists) return BadRequest("invalid");
			var results = await  _responseResolver.WithAttempts(1).WithResolverAsync(
				()=> _commentLogic.TranslateCommentsAsync(commentIds.ToArray()));
			return ResolverResponse(results);
		}
		
		[HttpPut]
		[Route("api/comments/unblock")]
		public async Task<IActionResult> UnblockUserCommenting([FromBody]List<long> userIds)
		{
			if (!_userContext.UserAccountExists) return BadRequest("invalid");
			var results = await _responseResolver.WithAttempts(1).WithResolverAsync(
				()=> _commentLogic.UnblockUserCommentingAsync(userIds.ToArray()));
			return ResolverResponse(results);
		}
		
		[HttpPut]
		[Route("api/comments/unlike/{commentId}")]
		public async Task<IActionResult> UnlikeComment([FromRoute]string commentId)
		{
			if (!_userContext.UserAccountExists) return BadRequest("invalid");
			var results = await  _responseResolver.WithAttempts(1).WithResolverAsync(
				()=> _commentLogic.UnlikeCommentAsync(commentId));
			return ResolverResponse(results);
		}
	}
}