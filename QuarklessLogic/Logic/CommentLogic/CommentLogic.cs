using InstagramApiSharp;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;
using QuarklessLogic.Handlers.ClientProvider;
using QuarklessLogic.Handlers.ReportHandler;
using System;
using System.Threading.Tasks;

namespace QuarklessLogic.Logic.CommentLogic
{
	public class CommentLogic : ICommentLogic
	{
		private IReportHandler _reportHandler { get; set; }
		private readonly IAPIClientContainer _Client;

		public CommentLogic(IReportHandler reportHandler, IAPIClientContainer client)
		{
			_reportHandler = reportHandler;
			_Client = client;
			_reportHandler.SetupReportHandler("Logic/Comments");
		}

		public async Task<IResult<bool>> BlockUserCommentingAsync(params long[] userIds)
		{
			try
			{
				return await _Client.Comment.BlockUserCommentingAsync(userIds);
			}
			catch(Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}

		public async Task<IResult<InstaComment>> CommentMediaAsync(string mediaId, string text)
		{
			try
			{
				return await _Client.Comment.CommentMediaAsync(mediaId,text);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}

		public async Task<IResult<bool>> DeleteCommentAsync(string mediaId, string commentId)
		{
			try
			{
				return await _Client.Comment.DeleteCommentAsync(mediaId, commentId);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}

		public async Task<IResult<bool>> DeleteMultipleCommentsAsync(string mediaId, params string[] commentIds)
		{
			try
			{
				return await _Client.Comment.DeleteMultipleCommentsAsync(mediaId, commentIds);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}

		public async Task<IResult<bool>> DisableMediaCommentAsync(string mediaId)
		{
			try
			{
				return await _Client.Comment.DisableMediaCommentAsync(mediaId);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}

		public async Task<IResult<bool>> EnableMediaCommentAsync(string mediaId)
		{
			try
			{
				return await _Client.Comment.EnableMediaCommentAsync(mediaId);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}

		public async Task<IResult<InstaUserShortList>> GetBlockedCommentersAsync()
		{
			try
			{
				return await _Client.Comment.GetBlockedCommentersAsync();
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}

		public async Task<IResult<InstaLikersList>> GetMediaCommentLikersAsync(string mediaId)
		{
			try
			{
				return await _Client.Comment.GetMediaCommentLikersAsync(mediaId);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}

		public async Task<IResult<InstaCommentList>> GetMediaCommentsAsync(string mediaId, int limit)
		{
			try
			{
				return await _Client.Comment.GetMediaCommentsAsync(mediaId, PaginationParameters.MaxPagesToLoad(limit));
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}

		public async Task<IResult<InstaInlineCommentList>> GetMediaRepliesCommentsAsync(string mediaId, string targetCommentId, int limit)
		{
			try
			{
				return await _Client.Comment.GetMediaRepliesCommentsAsync(mediaId, targetCommentId, PaginationParameters.MaxPagesToLoad(limit));
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}

		public async Task<IResult<bool>> LikeCommentAsync(string commentId)
		{
			try
			{
				return await _Client.Comment.LikeCommentAsync(commentId);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}

		public async Task<IResult<InstaComment>> ReplyCommentMediaAsync(string mediaId, string targetCommentId, string text)
		{
			try
			{
				return await _Client.Comment.ReplyCommentMediaAsync(mediaId, targetCommentId, text);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}

		public async Task<IResult<bool>> ReportCommentAsync(string mediaId, string commentId)
		{
			try
			{
				return await _Client.Comment.ReportCommentAsync(mediaId, commentId);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}

		public async Task<IResult<bool>> UnblockUserCommentingAsync(params long[] userIds)
		{
			try
			{
				return await _Client.Comment.UnblockUserCommentingAsync(userIds);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}

		public async Task<IResult<bool>> UnlikeCommentAsync(string commentId)
		{
			try
			{
				return await _Client.Comment.UnlikeCommentAsync(commentId);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}

		public async Task<IResult<InstaTranslateList>> TranslateCommentsAsync(params long[] commentIds)
		{
			try
			{
				return await _Client.Comment.TranslateCommentAsync(commentIds);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
	}
}
