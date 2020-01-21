using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;
using System.Threading.Tasks;
namespace Quarkless.Base.InstagramComments
{
	public interface ICommentLogic
	{
		Task<IResult<bool>> BlockUserCommentingAsync(params long[] userIds);
		Task<IResult<InstaComment>> CommentMediaAsync(string mediaId, string text);
		Task<IResult<bool>> DeleteCommentAsync(string mediaId, string commentId);
		Task<IResult<bool>> DeleteMultipleCommentsAsync(string mediaId, params string[] commentIds);
		Task<IResult<bool>> DisableMediaCommentAsync(string mediaId);
		Task<IResult<bool>> EnableMediaCommentAsync(string mediaId);
		Task<IResult<InstaUserShortList>> GetBlockedCommentersAsync();
		Task<IResult<InstaLikersList>> GetMediaCommentLikersAsync(string mediaId);
		Task<IResult<InstaCommentList>> GetMediaCommentsAsync(string mediaId, int limit);
		Task<IResult<InstaInlineCommentList>> GetMediaRepliesCommentsAsync(string mediaId, string targetCommentId, int limit);
		Task<IResult<bool>> LikeCommentAsync(string commentId);
		Task<IResult<InstaComment>> ReplyCommentMediaAsync(string mediaId, string targetCommentId, string text);
		Task<IResult<bool>> ReportCommentAsync(string mediaId, string commentId);
		Task<IResult<InstaTranslateList>> TranslateCommentsAsync(params long[] commentIds);
		Task<IResult<bool>> UnblockUserCommentingAsync(params long[] userIds);
		Task<IResult<bool>> UnlikeCommentAsync(string commentId);
	}
}