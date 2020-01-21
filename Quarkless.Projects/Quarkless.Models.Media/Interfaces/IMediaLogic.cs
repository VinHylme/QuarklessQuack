using System;
using System.Threading.Tasks;
using InstagramApiSharp;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;

namespace Quarkless.Models.Media.Interfaces
{
	public interface IMediaLogic
	{
		Task<IResult<bool>> ArchiveMediaAsync(string mediaId);
		Task<IResult<bool>> DeleteMediaAsync(string mediaId, int mediaType);
		Task<IResult<InstaMedia>> EditMediaAsync(string mediaId, EditMediaModel editMedia);
		Task<IResult<InstaMediaList>> GetArchivedMediaAsync(PaginationParameters paginationParameters);
		Task<IResult<InstaMediaIdList>> GetBlockedMediasAsync();
		Task<IResult<InstaMedia>> GetMediaByIdAsync(string mediaId);
		Task<IResult<InstaMediaList>> GetMediaByIdsAsync(params string[] mediaIds);
		Task<IResult<string>> GetMediaIdFromUrlAsync(Uri uri);
		Task<IResult<InstaLikersList>> GetMediaLikersAsync(string mediaId);
		Task<IResult<Uri>> GetShareLinkFromMediaIdAsync(string mediaId);
		Task<IResult<bool>> LikeMediaAsync(string mediaId);
		Task<IResult<bool>> ReportMediaAsync(string mediaId);
		Task<IResult<bool>> SaveMediaAsync(string mediaId);
		Task<IResult<bool>> UnArchiveMediaAsync(string mediaId);
		Task<IResult<bool>> UnLikeMediaAsync(string mediaId);
		Task<IResult<bool>> UnSaveMediaAsync(string mediaId);
		Task<IResult<InstaMedia>> UploadAlbumAsync(UploadAlbumModel uploadAlbum);
		Task<IResult<InstaMedia>> UploadPhotoAsync(UploadPhotoModel uploadPhoto);
		Task<IResult<InstaMedia>> UploadVideoAsync(UploadVideoModel uploadVideo);
	}
}