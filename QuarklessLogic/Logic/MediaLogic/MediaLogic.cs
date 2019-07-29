using InstagramApiSharp;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;
using QuarklessContexts.Models.MediaModels;
using QuarklessLogic.Handlers.ClientProvider;
using QuarklessLogic.Handlers.ReportHandler;
using Quarkless.MediaAnalyser;
using System;
using System.Threading.Tasks;

namespace QuarklessLogic.Logic.MediaLogic
{
	public class MediaLogic : IMediaLogic
	{
		private IReportHandler _reportHandler { get; set; }
		private readonly IAPIClientContainer _Client;

		public MediaLogic(IReportHandler reportHandler, IAPIClientContainer client)
		{
			_reportHandler = reportHandler;
			_Client = client;
			_reportHandler.SetupReportHandler("Logic/Comments");
		}
		public async Task<IResult<bool>> ArchiveMediaAsync(string mediaId)
		{
			try
			{
				return await _Client.Media.ArchiveMediaAsync(mediaId);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<bool>> DeleteMediaAsync(string mediaId, int mediaType)
		{
			try
			{
				return await _Client.Media.DeleteMediaAsync(mediaId,(InstaMediaType)mediaType);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaMedia>> EditMediaAsync(string mediaId,EditMediaModel editMedia)
		{
			try
			{
				return await _Client.Media.EditMediaAsync(mediaId,editMedia.Caption,editMedia.Location,editMedia.UserTags);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public Task<IResult<InstaMediaList>> GetArchivedMediaAsync(PaginationParameters paginationParameters)
		{
			throw new NotImplementedException();
		}

		public Task<IResult<InstaMediaIdList>> GetBlockedMediasAsync()
		{
			throw new NotImplementedException();
		}

		public Task<IResult<InstaMedia>> GetMediaByIdAsync(string mediaId)
		{
			throw new NotImplementedException();
		}

		public Task<IResult<InstaMediaList>> GetMediaByIdsAsync(params string[] mediaIds)
		{
			throw new NotImplementedException();
		}

		public Task<IResult<string>> GetMediaIdFromUrlAsync(Uri uri)
		{
			throw new NotImplementedException();
		}

		public Task<IResult<InstaLikersList>> GetMediaLikersAsync(string mediaId)
		{
			throw new NotImplementedException();
		}

		public Task<IResult<Uri>> GetShareLinkFromMediaIdAsync(string mediaId)
		{
			throw new NotImplementedException();
		}

		public async Task<IResult<bool>> LikeMediaAsync(string mediaId)
		{
			try
			{
				return await _Client.Media.LikeMediaAsync(mediaId);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}

		public Task<IResult<bool>> ReportMediaAsync(string mediaId)
		{
			throw new NotImplementedException();
		}

		public Task<IResult<bool>> SaveMediaAsync(string mediaId)
		{
			throw new NotImplementedException();
		}

		public Task<IResult<bool>> UnArchiveMediaAsync(string mediaId)
		{
			throw new NotImplementedException();
		}

		public async Task<IResult<bool>> UnLikeMediaAsync(string mediaId)
		{
			try
			{
				return await _Client.Media.UnLikeMediaAsync(mediaId);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}

		public Task<IResult<bool>> UnSaveMediaAsync(string mediaId)
		{
			throw new NotImplementedException();
		}

		public async Task<IResult<InstaMedia>> UploadAlbumAsync(UploadAlbumModel uploadAlbum)
		{
			try
			{
				return await _Client.Media.UploadAlbumAsync(uploadAlbum.Album,uploadAlbum.Caption,uploadAlbum.Location);
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaMedia>> UploadPhotoAsync(UploadPhotoModel uploadPhoto)
		{
			try { 
				return await _Client.Media.UploadPhotoAsync(uploadPhoto.Image, uploadPhoto.Caption, uploadPhoto.Location);
			}
			catch(Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
		public async Task<IResult<InstaMedia>> UploadVideoAsync(UploadVideoModel uploadVideo)
		{
			try
			{
				var res =  await  _Client.Media.UploadVideoAsync(uploadVideo.Video, uploadVideo.Caption, uploadVideo.Location);
				if (res.Succeeded)
				{
					Helper.DisposeVideos(uploadVideo.Video.Video.Uri);
				}
				return res;
			}
			catch (Exception ee)
			{
				_reportHandler.MakeReport(ee);
				return null;
			}
		}
	}
}
