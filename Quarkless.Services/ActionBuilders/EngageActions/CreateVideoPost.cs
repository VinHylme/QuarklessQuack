using Quarkless.Services.Interfaces;
using QuarklessContexts.Models.MediaModels;
using QuarklessContexts.Models.Profiles;
using System;
using System.Collections.Generic;
using System.Linq;
using Quarkless.MediaAnalyser;
using QuarklessContexts.Extensions;
using System.Threading.Tasks;
using InstagramApiSharp.Classes.Models;
using Newtonsoft.Json;
using QuarklessLogic.Handlers.RequestBuilder.Consts;
using QuarklessContexts.Enums;
using QuarklessContexts.Models.Timeline;

namespace Quarkless.Services.ActionBuilders.EngageActions
{
	public class CreateVideoPost : IActionCommit
	{
		private readonly ProfileModel _profile;
		private readonly DateTime _executeTime;
		private readonly IContentManager _builder;
		private const int VIDEO_FETCH_LIMIT = 25;
		public CreateVideoPost(IContentManager builder, ProfileModel profile, DateTime executeTime)
		{
			_builder = builder;
			_profile = profile;
			_executeTime = executeTime;
		}

		public void Operate()
		{
			string exactSize = _profile.AdditionalConfigurations.PostSize;
			var location = _profile.LocationTargetList?.ElementAtOrDefault(SecureRandom.Next(_profile.LocationTargetList.Count));
			var profileColor = _profile.Theme.Colors.ElementAt(SecureRandom.Next(0, _profile.Theme.Colors.Count));
			var topics = _builder.GetTopics(_profile.TopicList, 15).GetAwaiter().GetResult();
			var topicSelect = topics.ElementAt(SecureRandom.Next(0, topics.Count));

			List<string> pickedSubsTopics = topicSelect.SubTopics.TakeAny(3).ToList();
			pickedSubsTopics.Add(topicSelect.TopicName);
			var videos = _builder.GetMediaInstagram(InstaMediaType.Video, pickedSubsTopics, 1).ToList();

			if (videos.Count <= 0) return;
			List<byte[]> videoBytes = new List<byte[]>();
			Parallel.ForEach(videos.ElementAtOrDefault(SecureRandom.Next(videos.Count - 1)).MediaData, media =>
			{
				videoBytes.Add(media.DownloadMedia());
			});

			System.Drawing.Color profileColorRGB = System.Drawing.Color.FromArgb(profileColor.Alpha, profileColor.Red, profileColor.Green, profileColor.Blue);
			var selectVideo = profileColorRGB.MostSimilarVideo(videoBytes.Where(_ => _ != null).ToList(), 10);

			UploadVideoModel uploadVideo = new UploadVideoModel
			{
				Caption = _builder.GenerateMediaInfo(topicSelect, _profile.Language),
				Location = location != null ? new InstaLocationShort
				{
					Address = location.Address,
					Lat = location.Coordinates.Latitude,
					Lng = location.Coordinates.Longitude,
					Name = location.City
				} : null,
				Video = new InstaVideoUpload
				{
					Video = new InstaVideo
					{
						VideoBytes = selectVideo
					},
					VideoThumbnail = new InstaImage
					{
						ImageBytes = selectVideo.GenerateVideoThumbnail(8)
					}
				}
			};

			RestModel restModel = new RestModel
			{
				BaseUrl = UrlConstants.UploadVideo,
				RequestType = RequestType.POST,
				JsonBody = JsonConvert.SerializeObject(uploadVideo, Formatting.Indented)
			};
			_builder.AddToTimeline(restModel, _executeTime);
		}

		public void Operate<TActionType>(TActionType actionType = default(TActionType))
		{
			throw new NotImplementedException();
		}
	}

}
