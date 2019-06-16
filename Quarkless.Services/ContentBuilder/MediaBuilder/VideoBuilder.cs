using Quarkless.Queue.Jobs.JobOptions;
using Quarkless.Services.Extensions;
using Quarkless.Services.Interfaces;
using QuarklessContexts.Models.MediaModels;
using QuarklessContexts.Models.Profiles;
using System;
using System.Collections.Generic;
using System.Linq;
using Quarkless.MediaAnalyser;
namespace Quarkless.Services.ContentBuilder.MediaBuilder
{
	public class VideoBuilder : IContent
	{
		private readonly ProfileModel _profile;
		private readonly UserStore _userSession;
		private readonly DateTime _executeTime;
		private readonly IContentBuilderManager _builder;
		private const int VIDEO_FETCH_LIMIT = 25;
		private Random _random;
		public VideoBuilder(UserStore userSession, IContentBuilderManager builder, ProfileModel profile, DateTime executeTime)
		{
			_builder = builder;
			_profile = profile;
			_executeTime = executeTime;
			_userSession = userSession;
			_random = new Random(TimeSpan.FromMilliseconds(1000000).Milliseconds);
		}

		public void Operate()
		{
			string exactSize = _profile.AdditionalConfigurations.PostSize;
			var profileColor = _profile.Theme.Colors.ElementAt(_random.Next(0, _profile.Theme.Colors.Count - 1));
			var topics = _builder.GetTopics(_userSession, _profile.TopicList, 5).GetAwaiter().GetResult();
			var topicSelect = topics.ElementAt(_random.Next(0, topics.Count - 1));
			var subtopics = topics.Select(a => a.SubTopics).SquashMe().ToList();
			var videos = _builder.GetMediaInstagram(_userSession, InstagramApiSharp.Classes.Models.InstaMediaType.Video, subtopics,1).ToList();

			if(videos.Count<=0) return;
			List<byte[]> videoBytes = new List<byte[]>();
			videos.ElementAtOrDefault(_random.Next(0,videos.Count-1)).MediaData.ForEach(a=>videoBytes.Add(a.DownloadMedia()));

			System.Drawing.Color profileColorRGB = System.Drawing.Color.FromArgb(profileColor.Alpha, profileColor.Red, profileColor.Green, profileColor.Blue);
			var selectVideo = profileColorRGB.MostSimilarVideo(videoBytes.Where(_=>_!=null).ToList(),10);

			UploadVideoModel uploadPhoto = new UploadVideoModel
			{
				Caption = "a warriors pride",
					Video = new InstagramApiSharp.Classes.Models.InstaVideoUpload{ Video = new InstagramApiSharp.Classes.Models.InstaVideo 
					{
						VideoBytes = selectVideo
					},
					VideoThumbnail = new InstagramApiSharp.Classes.Models.InstaImage 
					{ ImageBytes = selectVideo.GenerateVideoThumbnail(8)}
				}
			};

			//RestModel restModel = new RestModel
			//{
			//	User = _userSession,
			//	BaseUrl = UrlConstants.UploadPhoto,
			//	RequestType = RequestType.POST,
			//	JsonBody = JsonConvert.SerializeObject(uploadPhoto)
			//};
			//_builder.AddToTimeline(restModel, _executeTime);
		}
	}
}
