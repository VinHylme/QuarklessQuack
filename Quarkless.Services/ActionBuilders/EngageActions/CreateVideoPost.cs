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
using Quarkless.Services.Interfaces.Actions;
using Quarkless.Services.StrategyBuilders;
using QuarklessContexts.Models;
using QuarklessLogic.ServicesLogic.HeartbeatLogic;
using QuarklessContexts.Models.ServicesModels.SearchModels;
using QuarklessContexts.Models.ServicesModels.HeartbeatModels;
using QuarklessContexts.Classes.Carriers;

namespace Quarkless.Services.ActionBuilders.EngageActions
{
	public class CreateVideoPost : IActionCommit
	{
		private readonly ProfileModel _profile;
		private UserStoreDetails user;
		private readonly IContentManager _builder;
		private readonly IHeartbeatLogic _heartbeatLogic;
		private VideoStrategySettings videoStrategy { get; set; }
		public CreateVideoPost(IContentManager builder, IHeartbeatLogic heartbeatLogic,ProfileModel profile)
		{
			_builder = builder;
			_profile = profile;
			_heartbeatLogic = heartbeatLogic;
		}

		public IActionCommit IncludeStrategy(IStrategySettings strategy)
		{
			this.videoStrategy = strategy as VideoStrategySettings;
			return this;
		}

		public ResultCarrier<IEnumerable<TimelineEventModel>> Push(IActionOptions actionOptions)
		{
			VideoActionOptions videoActionOptions = actionOptions as VideoActionOptions;
			ResultCarrier<IEnumerable<TimelineEventModel>> Results = new ResultCarrier<IEnumerable<TimelineEventModel>>();

			if (user==null){
				Results.IsSuccesful = false;
				Results.Info = new ErrorResponse
				{
					Message = $"user is null, user: {user.OAccountId}, instaId: {user.OInstagramAccountUsername}",
					StatusCode = System.Net.HttpStatusCode.NotFound
				};
				return Results;
			}

			string exactSize = _profile.AdditionalConfigurations.PostSize;
			var location = _profile.LocationTargetList?.ElementAtOrDefault(SecureRandom.Next(_profile.LocationTargetList.Count));

			var profileColor = _profile.Theme.Colors.ElementAt(SecureRandom.Next(0, _profile.Theme.Colors.Count));

			var topic_ = _builder.GetTopic(_profile).GetAwaiter().GetResult();

			var media = _heartbeatLogic.GetMetaData<Media>(MetaDataType.FetchMediaByTopic,_profile.Topics.TopicFriendlyName).GetAwaiter().GetResult();
			var videos = media.Select(s =>
				new __Meta__<Media>(new Media
				{
					Medias = s.Value.ObjectItem.Medias.Where(x => x.MediaType == InstaMediaType.Image).ToList(),
					errors = s.Value.ObjectItem.errors
				})).ToList();

			if (videos.Count <= 0)
			{
				Results.IsSuccesful = false;
				Results.Info = new ErrorResponse
				{
					Message = $"no videos found, user: {user.OAccountId}, instaId: {user.OInstagramAccountUsername}",
					StatusCode = System.Net.HttpStatusCode.NotFound
				};
				return Results;
			}
			List<byte[]> videoBytes = new List<byte[]>();

			Parallel.ForEach(videos, v =>
			{
				videoBytes.Add(v.ObjectItem.Medias.FirstOrDefault().MediaUrl.First().DownloadMedia());
			});

			System.Drawing.Color profileColorRGB = System.Drawing.Color.FromArgb(profileColor.Alpha, profileColor.Red, profileColor.Green, profileColor.Blue);
			var selectVideo = profileColorRGB.MostSimilarVideo(videoBytes.Where(_ => _ != null).ToList(), 10);

			UploadVideoModel uploadVideo = new UploadVideoModel
			{
				Caption = _builder.GenerateMediaInfo(topic_, _profile.Language),
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
				JsonBody = JsonConvert.SerializeObject(uploadVideo, Formatting.Indented),
				User = user
			};
			Results.IsSuccesful = true;
			Results.Results = new List<TimelineEventModel>
			{
				new TimelineEventModel
				{
					ActionName = $"CreateVideo_{videoStrategy.VideoStrategyType.ToString()}",
					Data = restModel,
					ExecutionTime = videoActionOptions.ExecutionTime
				}
			};
			return Results;
		}

		public IActionCommit IncludeUser(UserStoreDetails userStoreDetails)
		{
			user = userStoreDetails;
			return this;
		}
	}

}
