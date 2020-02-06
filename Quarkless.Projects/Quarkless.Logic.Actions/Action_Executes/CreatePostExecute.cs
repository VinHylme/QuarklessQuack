using System;
using System.Linq;
using System.Threading.Tasks;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;
using Newtonsoft.Json;
using Quarkless.Analyser;
using Quarkless.Models.Actions;
using Quarkless.Models.Actions.Interfaces;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Common.Extensions;
using Quarkless.Models.Common.Models.Carriers;
using Quarkless.Models.Media;
using Quarkless.Models.ResponseResolver.Interfaces;
using Quarkless.Models.ResponseResolver.Models;
using Quarkless.Models.WorkerManager.Interfaces;

namespace Quarkless.Logic.Actions.Action_Executes
{
	internal class CreatePostExecute: IActionExecute
	{
		private readonly IWorker _worker;
		private readonly IResponseResolver _responseResolver;
		private readonly IPostAnalyser _postAnalyser;
		internal CreatePostExecute(IWorker worker, IResponseResolver responseResolver, IPostAnalyser postAnalyser)
		{
			_worker = worker;
			_responseResolver = responseResolver;
			_postAnalyser = postAnalyser;
		}
		private string MakeCaption(MediaInfo mediaInfo)
		{
			var hashtags = mediaInfo.Hashtags.Select(j => j.Replace(" ", "")).JoinEvery(Environment.NewLine, 3);
			var creditLine = string.Empty;

			if (mediaInfo.Credit != null)
				creditLine = $"@{mediaInfo.Credit}";
			var separate = "\n.\n.\n.\n";
			return mediaInfo.Caption + separate + creditLine + Environment.NewLine + hashtags;
		}

		public async Task<ResultCarrier<bool>> ExecuteAsync(EventExecuteBody eventAction)
		{
			var result = new ResultCarrier<bool>();
			try
			{
				Console.WriteLine($"Started to execute {GetType().Name} for {_worker.WorkerAccountId}/{_worker.WorkerUsername}");
				
				ResolverResponse<InstaMedia> response;
				if (eventAction.BodyType == typeof(UploadPhotoModel))
				{
					var model = JsonConvert.DeserializeObject<UploadPhotoModel>(eventAction.Body.ToJsonString());
					
					model.Image.ImageBytes = _postAnalyser.Manipulation.ImageEditor
						.ResizeToClosestAspectRatio(_postAnalyser.Manager
							.DownloadMedia(model.Image.Uri));

					model.Image.Uri = string.Empty;

					response = await _responseResolver
						.WithClient(_worker.Client)
						.WithAttempts(1)
						.WithResolverAsync(()=> _worker.Client.Media
								.UploadPhotoAsync(model.Image, MakeCaption(model.MediaInfo), model.Location),
							ActionType.CreatePost, model.ToJsonString());
				}
				else if(eventAction.BodyType == typeof(UploadVideoModel))
				{
					var model = JsonConvert.DeserializeObject<UploadVideoModel>(eventAction.Body.ToJsonString());
					model.Video.Video.VideoBytes = _postAnalyser.Manager.DownloadMedia(model.Video.Video.Uri);
					model.Video.Video.Uri = string.Empty;

					response = await _responseResolver
						.WithClient(_worker.Client)
						.WithAttempts(1)
						.WithResolverAsync(()=> _worker.Client.Media
								.UploadVideoAsync(model.Video, MakeCaption(model.MediaInfo), model.Location),
							ActionType.CreatePost, model.ToJsonString());
				}
				else if (eventAction.BodyType == typeof(UploadAlbumModel))
				{
					var model = JsonConvert.DeserializeObject<UploadAlbumModel>(eventAction.Body.ToJsonString());
					foreach (var instaAlbumUpload in model.Album)
					{
						if (instaAlbumUpload.VideoToUpload != null)
						{
							instaAlbumUpload.VideoToUpload.Video.VideoBytes =
								_postAnalyser.Manager.DownloadMedia(instaAlbumUpload.VideoToUpload.Video.Uri);
							instaAlbumUpload.VideoToUpload.Video.Uri = string.Empty;
						}

						else
						{
							instaAlbumUpload.ImageToUpload.ImageBytes =
								_postAnalyser.Manager.DownloadMedia(instaAlbumUpload.ImageToUpload.Uri);
							instaAlbumUpload.ImageToUpload.Uri = string.Empty;
						}
					}

					response = await _responseResolver
						.WithClient(_worker.Client)
						.WithAttempts(1)
						.WithResolverAsync(()=> _worker.Client.Media
								.UploadAlbumAsync(model.Album, MakeCaption(model.MediaInfo), model.Location),
							ActionType.CreatePost, model.ToJsonString());
				}
				else
				{
					throw new Exception("Invalid Model");
				}
				
				if (response == null)
				{
					throw new Exception("Response returned null");
				}

				if (!response.Response.Succeeded)
				{
					result.IsSuccessful = false;
					result.Info = new ErrorResponse
					{
						Message = response.Response.Info.Message,
						Exception = response.Response.Info.Exception
					};
					return result;
				}

				result.IsSuccessful = true;
				result.Results = true;
				return result;
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
				result.IsSuccessful = false;
				result.Info = new ErrorResponse
				{
					Message = err.Message,
					Exception = err
				};
				return result;
			}
			finally
			{
				Console.WriteLine($"Ended execute {GetType().Name} for {_worker.WorkerAccountId}/{_worker.WorkerUsername} Was Successful: {result.IsSuccessful}");
			}
		}
	}
}
