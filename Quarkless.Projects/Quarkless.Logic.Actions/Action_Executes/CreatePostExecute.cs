using System;
using System.Linq;
using System.Threading.Tasks;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;
using Quarkless.Models.Actions;
using Quarkless.Models.Actions.Interfaces;
using Quarkless.Models.Common.Enums;
using Quarkless.Models.Common.Extensions;
using Quarkless.Models.Common.Models.Carriers;
using Quarkless.Models.Media;
using Quarkless.Models.ResponseResolver.Interfaces;
using Quarkless.Models.WorkerManager.Interfaces;

namespace Quarkless.Logic.Actions.Action_Executes
{
	internal class CreatePostExecute: IActionExecute
	{
		private readonly IWorker _worker;
		private readonly IResponseResolver _responseResolver;
		internal CreatePostExecute(IWorker worker, IResponseResolver responseResolver)
		{
			_worker = worker;
			_responseResolver = responseResolver;
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
				Console.WriteLine($"Started to execute {nameof(GetType)} for {_worker.WorkerAccountId}/{_worker.WorkerUsername}");
				IResult<InstaMedia> response;
				switch (eventAction.Body)
				{
					case UploadPhotoModel model:
					{
						model.Image.Uri = string.Empty;

						response = await _responseResolver.WithClient(_worker.Client)
							.WithResolverAsync(await _worker.Client.Media
								.UploadPhotoAsync(model.Image, MakeCaption(model.MediaInfo), model.Location),
								ActionType.CreatePost, model.ToJsonString());
						break;
					}
					case UploadVideoModel model:
					{
						model.Video.Video.Uri = string.Empty;

						response = await _responseResolver.WithClient(_worker.Client)
							.WithResolverAsync(await _worker.Client.Media
								.UploadVideoAsync(model.Video, MakeCaption(model.MediaInfo), model.Location), 
								ActionType.CreatePost, model.ToJsonString());
						break;
					}
					case UploadAlbumModel model:
					{
						foreach (var instaAlbumUpload in model.Album)
						{
							if (instaAlbumUpload.VideoToUpload != null)
								instaAlbumUpload.VideoToUpload.Video.Uri = string.Empty;
							
							else
								instaAlbumUpload.ImageToUpload.Uri = string.Empty;
						}

						response = await _responseResolver.WithClient(_worker.Client)
						.WithResolverAsync(await _worker.Client.Media
							.UploadAlbumAsync(model.Album, MakeCaption(model.MediaInfo), model.Location), 
							ActionType.CreatePost, model.ToJsonString());
						break;
					}
					default: throw new Exception("Invalid Model");
				}

				if (response == null)
				{
					throw new Exception("Response returned null");
				}

				if (!response.Succeeded)
				{
					result.IsSuccessful = false;
					result.Info = new ErrorResponse
					{
						Message = response.Info.Message,
						Exception = response.Info.Exception
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
				Console.WriteLine($"Ended execute {nameof(GetType)} for {_worker.WorkerAccountId}/{_worker.WorkerUsername}");
			}
		}
	}
}
