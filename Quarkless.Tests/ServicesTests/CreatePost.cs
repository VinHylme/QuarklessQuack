using System.Collections.Generic;
using System.Threading.Tasks;
using InstagramApiSharp.Classes.Models;
using NUnit.Framework;
using Moq;
using NUnit.Framework.Constraints;
using Quarkless.MediaAnalyser;
using Quarkless.Services.Interfaces;
using QuarklessContexts.Models.MediaModels;
using QuarklessContexts.Models.ServicesModels.HeartbeatModels;
using QuarklessContexts.Models.ServicesModels.SearchModels;
using QuarklessContexts.Models.Timeline;
using QuarklessLogic.Handlers.ClientProvider;
using QuarklessLogic.Handlers.ReportHandler;
using QuarklessLogic.Logic.MediaLogic;
using QuarklessLogic.Logic.ProfileLogic;
using QuarklessLogic.ServicesLogic.HeartbeatLogic;
using QuarklessRepositories.ProfileRepository;

namespace Quarkless.Tests.ServicesTests
{
	class CreatePost
	{
		private IContentManager _contentManager;
		private IHeartbeatLogic _heartbeatLogic;
		private IProfileLogic _profileLogic;
		private IMediaLogic _mediaLogic;
		private UserStoreDetails user;
		private ServiceReacher _services;
		private UploadVideoModel videoModelTest;
		private string localvideoPath = @"C:\Users\yousef.alaw\source\repos\QuarklessQuark\Quarkless.Tests\data\nature.mp4";
		[SetUp]
		public void Setup()
		{
			_services = new Init().InitialiseContext();
			_contentManager = _services.Get<IContentManager>();
			_heartbeatLogic = _services.Get<IHeartbeatLogic>();
			_profileLogic = _services.Get<IProfileLogic>();
			InitialiseDummyData().GetAwaiter().GetResult();
			_mediaLogic = new MediaLogic(_services.Get<IReportHandler>(), 
				new APIClientContainer(_services.Get<IAPIClientContext>(),user.OAccountId,user.OInstagramAccountUser));
		}

		private async Task InitialiseDummyData()
		{
			user = new UserStoreDetails
			{
				OAccountId = "lemonkaces",
				OInstagramAccountUser = "5cf3d6b9871f49057c0169bc",
				OAccessToken = "eyJraWQiOiJZQ3p6dUdsbXI2UmNBc3o1N2RsTFA1azF4ZmpNNlNuS21EVmppYUVlT3ZvPSIsImFsZyI6IlJTMjU2In0.eyJzdWIiOiI0ZGYyMDM5Ni0zZjI4LTQ4ZjctYjQ5Zi05NzU5NTk5Y2U5MTEiLCJjb2duaXRvOmdyb3VwcyI6WyJBZG1pbiJdLCJlbWFpbF92ZXJpZmllZCI6dHJ1ZSwiaXNzIjoiaHR0cHM6XC9cL2NvZ25pdG8taWRwLmV1LXdlc3QtMi5hbWF6b25hd3MuY29tXC9ldS13ZXN0LTJfQ2lBdHR1RW0zIiwiY29nbml0bzp1c2VybmFtZSI6ImxlbW9ua2FjZXMiLCJhdWQiOiI1ZzJqMWYzNzI3NW0yMnFxbHNlcDV1c3QyciIsImV2ZW50X2lkIjoiODU3NmQxMzItNDE4Yi00M2UxLTlhNmYtNzNiNDAyNTAzMWQzIiwidG9rZW5fdXNlIjoiaWQiLCJhdXRoX3RpbWUiOjE1NjkzODk1ODgsIm5hbWUiOiJ0cnVldG9mYW4iLCJleHAiOjE1Njk0OTU1OTUsImlhdCI6MTU2OTQ5MTk5NSwiZW1haWwiOiJvdXNlYmRvdW5pYUBnbWFpbC5jb20ifQ.gP75nG6Bw0B5QFAg8rSFWwTVEqszjJFNZn7vkj3P6KmGBVLMrfXVeV3GFRi0mfTGMB6i_TAR0wmCrzUNfiHwhInZf8sbbkUWIVlmCIWFrgbwE4V6o_9DYY2eNcW4q3-zILCgEcK7jdz6H6vRVPknL1hEyPsnVqz2EEaKUDjif_H1w8ElTZ1G-_ggWmyWVEQp63Y61Yd36nfm43BDmdsZMJKvF98XMkhtOBy4ZIqf00HXy4zipTOKIxWbxapn9yOX1iz6mAGHKPvY-y8Awb3yKFKMXblZI7QhiyGQOpD5fwoPDVC5iL6ELcTg6Mdh2yIHPX4N-xgf8MbQYCRhNuWf8A",
				Profile = await _profileLogic.GetProfile("lemonkaces","5cf3d6b9871f49057c0169bc")
			};

			videoModelTest = new UploadVideoModel
			{
				Location = new InstaLocationShort
				{
					Address = "manchester",
					Name = "manchester city"
				},
				MediaInfo = new MediaInfo
				{
					Caption = "wow so stunning",
					Hashtags = new List<string>
					{
						"yanart", "maluma", "#medellin", "#art" , "#artist", "#draw", "#drawing", "#instaart", "#urbanart", "#graffiti"
					}
				},
				Video = new InstaVideoUpload
				{
					UserTags = null,
					Video = new InstaVideo
					{
						//Uri = localvideoPath,
						VideoBytes = localvideoPath.DownloadMediaLocal(),
					},
					VideoThumbnail = new InstaImage
					{
						ImageBytes = await localvideoPath.DownloadMediaLocal().GenerateVideoThumbnail()
					}
				}
			};
		}

		[Test]
		public async Task Test1()
		{
			var results = await _mediaLogic.UploadVideoAsync(videoModelTest);
			Assert.IsTrue(results.Succeeded);
		}

		[Test]
		public void ConcurrencyTest()
		{
			Parallel.For(0,10, async i => { await InitialiseDummyData(); });
		}
	}
}
